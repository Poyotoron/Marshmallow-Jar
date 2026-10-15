using System;
using System.Collections.Generic;

namespace Poyo.MarshmallowJar.Editor
{
    internal sealed class WebhookJsonNode
    {
        internal WebhookJsonKind Kind;
        internal string Text;
        internal readonly List<string> Keys = new List<string>();
        internal readonly List<WebhookJsonNode> Children = new List<WebhookJsonNode>();

        internal int IndexOf(string key)
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                if (string.Equals(Keys[i], key, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        internal static WebhookJsonNode CreateString(string text)
        {
            return new WebhookJsonNode { Kind = WebhookJsonKind.String, Text = text };
        }

        internal static WebhookJsonNode CreateRaw(string text)
        {
            return new WebhookJsonNode { Kind = WebhookJsonKind.Raw, Text = text };
        }

        internal static WebhookJsonNode CreateObject()
        {
            return new WebhookJsonNode { Kind = WebhookJsonKind.Object };
        }

        internal static WebhookJsonNode CreateArray()
        {
            return new WebhookJsonNode { Kind = WebhookJsonKind.Array };
        }
    }
}
