package bomberman.network;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/**
 * Minimal JSON reader for the SignalR JSON hub protocol.
 * This client only sends fixed method names and integer arguments; no JSON library is required.
 */
final class SignalRJson {
    private final String source;
    private int index;

    private SignalRJson(String source) {
        this.source = source;
    }

    static Map<String, Object> parseMessage(String json) {
        SignalRJson reader = new SignalRJson(json);
        Object result = reader.value(0);
        reader.whitespace();
        if (reader.index != json.length() || !(result instanceof Map<?, ?> map)) {
            throw new IllegalArgumentException("Expected JSON object");
        }
        @SuppressWarnings("unchecked")
        Map<String, Object> typed = (Map<String, Object>) map;
        return typed;
    }

    private Object value(int depth) {
        if (depth > 32) {
            throw new IllegalArgumentException("JSON nesting is too deep");
        }
        whitespace();
        if (index >= source.length()) {
            throw new IllegalArgumentException("Unexpected end of JSON");
        }
        char c = source.charAt(index);
        return switch (c) {
            case '{' -> object(depth + 1);
            case '[' -> array(depth + 1);
            case '"' -> string();
            case 't' -> keyword("true", Boolean.TRUE);
            case 'f' -> keyword("false", Boolean.FALSE);
            case 'n' -> keyword("null", null);
            default -> {
                if (c == '-' || (c >= '0' && c <= '9')) {
                    yield number();
                }
                throw new IllegalArgumentException("Invalid JSON at offset " + index);
            }
        };
    }

    private Map<String, Object> object(int depth) {
        expect('{');
        Map<String, Object> map = new LinkedHashMap<>();
        whitespace();
        if (eat('}')) return map;
        do {
            whitespace();
            if (index >= source.length() || source.charAt(index) != '"') {
                throw new IllegalArgumentException("JSON object key must be a string");
            }
            String key = string();
            whitespace();
            expect(':');
            map.put(key, value(depth));
            whitespace();
            if (eat('}')) return map;
            expect(',');
        } while (true);
    }

    private List<Object> array(int depth) {
        expect('[');
        List<Object> result = new ArrayList<>();
        whitespace();
        if (eat(']')) return result;
        do {
            result.add(value(depth));
            whitespace();
            if (eat(']')) return result;
            expect(',');
        } while (true);
    }

    private String string() {
        expect('"');
        StringBuilder out = new StringBuilder();
        while (index < source.length()) {
            char c = source.charAt(index++);
            if (c == '"') return out.toString();
            if (c == '\\') {
                if (index >= source.length()) throw new IllegalArgumentException("Invalid escape");
                char escaped = source.charAt(index++);
                switch (escaped) {
                    case '"', '\\', '/' -> out.append(escaped);
                    case 'b' -> out.append('\b');
                    case 'f' -> out.append('\f');
                    case 'n' -> out.append('\n');
                    case 'r' -> out.append('\r');
                    case 't' -> out.append('\t');
                    case 'u' -> {
                        if (index + 4 > source.length()) throw new IllegalArgumentException("Invalid unicode escape");
                        int code = Integer.parseInt(source.substring(index, index + 4), 16);
                        out.append((char) code);
                        index += 4;
                    }
                    default -> throw new IllegalArgumentException("Invalid escape sequence");
                }
            } else {
                if (c < ' ') throw new IllegalArgumentException("Control character in JSON string");
                out.append(c);
            }
        }
        throw new IllegalArgumentException("Unterminated JSON string");
    }

    private Object number() {
        int start = index;
        if (eat('-') && index >= source.length()) throw new IllegalArgumentException("Invalid number");
        if (eat('0')) {
            // Leading zero only allowed for the number zero.
        } else {
            if (!digit()) throw new IllegalArgumentException("Invalid number");
            while (digit()) index++;
        }
        boolean floating = false;
        if (eat('.')) {
            floating = true;
            if (!digit()) throw new IllegalArgumentException("Invalid number fraction");
            while (digit()) index++;
        }
        if (eat('e') || eat('E')) {
            floating = true;
            if (!eat('+')) eat('-');
            if (!digit()) throw new IllegalArgumentException("Invalid exponent");
            while (digit()) index++;
        }
        String literal = source.substring(start, index);
        try {
            if (floating) return Double.valueOf(literal);
            return Long.valueOf(literal);
        } catch (NumberFormatException exception) {
            throw new IllegalArgumentException("Invalid JSON number", exception);
        }
    }

    private boolean digit() {
        return index < source.length() && source.charAt(index) >= '0' && source.charAt(index) <= '9';
    }

    private Object keyword(String text, Object result) {
        if (!source.startsWith(text, index)) throw new IllegalArgumentException("Invalid JSON keyword");
        index += text.length();
        return result;
    }

    private void whitespace() {
        while (index < source.length()) {
            char c = source.charAt(index);
            if (c == ' ' || c == '\n' || c == '\r' || c == '\t') index++;
            else break;
        }
    }

    private boolean eat(char c) {
        if (index < source.length() && source.charAt(index) == c) {
            index++;
            return true;
        }
        return false;
    }

    private void expect(char c) {
        if (!eat(c)) throw new IllegalArgumentException("Expected '" + c + "' at offset " + index);
    }
}
