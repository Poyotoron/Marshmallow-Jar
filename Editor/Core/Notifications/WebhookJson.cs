using System.Text;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class WebhookJson
    {
        internal static bool TryParse(string text, out WebhookJsonNode root, out string error)
        {
            Parser parser = new Parser(text ?? string.Empty);
            if (!parser.TryReadValue(out root) || !parser.TryFinish())
            {
                root = null;
                error = parser.Error;
                return false;
            }
            error = null;
            return true;
        }

        internal static string Serialize(WebhookJsonNode root)
        {
            StringBuilder builder = new StringBuilder();
            WriteNode(builder, root, 0);
            return builder.ToString();
        }

        private static void WriteString(StringBuilder builder, string text)
        {
            builder.Append('"');
            WebhookSender.AppendJsonEscaped(builder, text);
            builder.Append('"');
        }

        private static void WriteNode(StringBuilder builder, WebhookJsonNode node, int indent)
        {
            if (node.Kind == WebhookJsonKind.String)
            {
                WriteString(builder, node.Text);
                return;
            }
            if (node.Kind == WebhookJsonKind.Raw)
            {
                builder.Append(node.Text);
                return;
            }
            bool isObject = node.Kind == WebhookJsonKind.Object;
            builder.Append(isObject ? '{' : '[');
            if (node.Children.Count == 0)
            {
                builder.Append(isObject ? '}' : ']');
                return;
            }
            bool inline = isObject && node.Children.Count == 1 &&
                (node.Children[0].Kind == WebhookJsonKind.String || node.Children[0].Kind == WebhookJsonKind.Raw);
            if (inline)
            {
                builder.Append(' ');
                WriteString(builder, node.Keys[0]);
                builder.Append(": ");
                WriteNode(builder, node.Children[0], indent);
                builder.Append(" }");
                return;
            }
            builder.Append('\n');
            for (int i = 0; i < node.Children.Count; i++)
            {
                builder.Append(' ', indent + 2);
                if (isObject)
                {
                    WriteString(builder, node.Keys[i]);
                    builder.Append(": ");
                }
                WriteNode(builder, node.Children[i], indent + 2);
                if (i + 1 < node.Children.Count)
                {
                    builder.Append(',');
                }
                builder.Append('\n');
            }
            builder.Append(' ', indent);
            builder.Append(isObject ? '}' : ']');
        }

        private sealed class Parser
        {
            private readonly string _text;
            private int _position;
            internal string Error { get; private set; }

            internal Parser(string text)
            {
                _text = text;
            }

            private void SkipWhitespace()
            {
                while (_position < _text.Length)
                {
                    char c = _text[_position];
                    if (c != ' ' && c != '\t' && c != '\r' && c != '\n')
                    {
                        break;
                    }
                    _position++;
                }
            }

            private bool Fail()
            {
                if (_position >= _text.Length)
                {
                    Error = "JSON が途中で終わっています";
                    return false;
                }
                int line = 1;
                int column = 1;
                for (int i = 0; i < _position; i++)
                {
                    if (_text[i] == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }
                }
                Error = line + " 行目 " + column + " 文字目に、予期しない文字があります";
                return false;
            }

            internal bool TryFinish()
            {
                SkipWhitespace();
                return _position == _text.Length || Fail();
            }

            internal bool TryReadValue(out WebhookJsonNode node)
            {
                node = null;
                SkipWhitespace();
                if (_position == _text.Length)
                {
                    return Fail();
                }
                char c = _text[_position];
                if (c == '"')
                {
                    if (!TryReadString(out string value))
                    {
                        return false;
                    }
                    node = WebhookJsonNode.CreateString(value);
                    return true;
                }
                if (c == '{' && _position + 1 < _text.Length && IsPlaceholderChar(_text[_position + 1]))
                {
                    int start = _position++;
                    while (_position < _text.Length && IsPlaceholderChar(_text[_position]))
                    {
                        _position++;
                    }
                    if (!TryConsume('}'))
                    {
                        return false;
                    }
                    node = WebhookJsonNode.CreateRaw(_text.Substring(start, _position - start));
                    return true;
                }
                if (c == '{' || c == '[')
                {
                    return TryReadContainer(c == '{', out node);
                }
                if (c == '-' || (c >= '0' && c <= '9'))
                {
                    int start = _position++;
                    while (_position < _text.Length && IsNumberChar(_text[_position]))
                    {
                        _position++;
                    }
                    node = WebhookJsonNode.CreateRaw(_text.Substring(start, _position - start));
                    return true;
                }
                string literal = c == 't' ? "true" : c == 'f' ? "false" : c == 'n' ? "null" : null;
                if (literal == null)
                {
                    return Fail();
                }
                for (int i = 0; i < literal.Length; i++)
                {
                    if (_position + i >= _text.Length)
                    {
                        _position = _text.Length;
                        return Fail();
                    }
                    if (_text[_position + i] != literal[i])
                    {
                        return Fail();
                    }
                }
                _position += literal.Length;
                node = WebhookJsonNode.CreateRaw(literal);
                return true;
            }

            private bool TryReadContainer(bool isObject, out WebhookJsonNode node)
            {
                node = isObject ? WebhookJsonNode.CreateObject() : WebhookJsonNode.CreateArray();
                char end = isObject ? '}' : ']';
                _position++;
                SkipWhitespace();
                if (_position < _text.Length && _text[_position] == end)
                {
                    _position++;
                    return true;
                }
                while (true)
                {
                    if (isObject)
                    {
                        if (!TryReadString(out string key))
                        {
                            return false;
                        }
                        SkipWhitespace();
                        if (!TryConsume(':'))
                        {
                            return false;
                        }
                        node.Keys.Add(key);
                    }
                    if (!TryReadValue(out WebhookJsonNode child))
                    {
                        return false;
                    }
                    node.Children.Add(child);
                    SkipWhitespace();
                    if (_position < _text.Length && _text[_position] == end)
                    {
                        _position++;
                        return true;
                    }
                    if (!TryConsume(','))
                    {
                        return false;
                    }
                    SkipWhitespace();
                }
            }

            private bool TryConsume(char expected)
            {
                if (_position >= _text.Length || _text[_position] != expected)
                {
                    return Fail();
                }
                _position++;
                return true;
            }

            private bool TryReadString(out string value)
            {
                value = null;
                if (!TryConsume('"'))
                {
                    return false;
                }
                StringBuilder builder = new StringBuilder();
                while (_position < _text.Length)
                {
                    char c = _text[_position];
                    if (c == '"')
                    {
                        _position++;
                        value = builder.ToString();
                        return true;
                    }
                    if (c < ' ')
                    {
                        return Fail();
                    }
                    _position++;
                    if (c != '\\')
                    {
                        builder.Append(c);
                        continue;
                    }
                    if (_position == _text.Length)
                    {
                        return Fail();
                    }
                    c = _text[_position];
                    switch (c)
                    {
                        case '"':
                            builder.Append('"');
                            break;
                        case '\\':
                            builder.Append('\\');
                            break;
                        case '/':
                            builder.Append('/');
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'u':
                            int decoded = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                _position++;
                                if (_position == _text.Length)
                                {
                                    return Fail();
                                }
                                char digit = _text[_position];
                                int hex = digit >= '0' && digit <= '9' ? digit - '0' :
                                    digit >= 'a' && digit <= 'f' ? digit - 'a' + 10 :
                                    digit >= 'A' && digit <= 'F' ? digit - 'A' + 10 : -1;
                                if (hex < 0)
                                {
                                    return Fail();
                                }
                                decoded = decoded * 16 + hex;
                            }
                            builder.Append((char)decoded);
                            break;
                        default:
                            return Fail();
                    }
                    _position++;
                }
                return Fail();
            }

            private static bool IsPlaceholderChar(char c)
            {
                return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') || c == '_';
            }

            private static bool IsNumberChar(char c)
            {
                return (c >= '0' && c <= '9') || c == '+' || c == '-' || c == '.' || c == 'e' || c == 'E';
            }
        }
    }
}
