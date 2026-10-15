using System;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class WebhookSimpleFields
    {
        internal const int Username = 0;
        internal const int AvatarUrl = 1;
        internal const int Content = 2;
        internal const int Title = 3;
        internal const int Description = 4;
        internal const int Footer = 5;
        internal const int Count = 6;
        private static readonly string[] RootOrder = { "username", "avatar_url", "content", "embeds" };
        private static readonly string[] EmbedOrder = { "title", "description", "color", "footer" };
        private static readonly string[] FooterOrder = { "text" };

        internal static WebhookSimpleSnapshot Read(string template)
        {
            if (!WebhookJson.TryParse(template, out WebhookJsonNode root, out string error))
            {
                return new WebhookSimpleSnapshot
                {
                    ErrorMessage = "JSON の書き方に誤りがあるため、かんたん編集では表示できません。JSON のタブで直してください（" + error + "）。",
                };
            }
            return ReadRoot(root);
        }

        private static WebhookSimpleSnapshot ReadRoot(WebhookJsonNode root)
        {
            WebhookSimpleSnapshot snapshot = new WebhookSimpleSnapshot();
            if (root.Kind != WebhookJsonKind.Object)
            {
                snapshot.ErrorMessage = "最上位が { } ではないため、かんたん編集では表示できません。JSON のタブで直してください。";
                return snapshot;
            }
            snapshot.HasOtherItems = HasOtherKeys(root, RootOrder);
            for (int i = Username; i <= Content; i++)
            {
                ReadField(root, RootOrder[i], i, snapshot);
            }
            WebhookJsonNode embeds = Get(root, "embeds");
            if (embeds == null || (embeds.Kind == WebhookJsonKind.Array && embeds.Children.Count == 0))
            {
                ReadEmbed(null, snapshot);
            }
            else if (embeds.Kind != WebhookJsonKind.Array || embeds.Children[0].Kind != WebhookJsonKind.Object)
            {
                snapshot.HasOtherItems = true;
            }
            else
            {
                snapshot.HasOtherItems |= embeds.Children.Count > 1;
                ReadEmbed(embeds.Children[0], snapshot);
            }
            for (int i = 0; i < Count; i++)
            {
                snapshot.HasOtherItems |= !snapshot.Editable[i];
                if (i >= Content && !string.IsNullOrEmpty(snapshot.Values[i]))
                {
                    snapshot.HasVisibleContent = true;
                }
            }
            return snapshot;
        }

        private static void ReadEmbed(WebhookJsonNode embed, WebhookSimpleSnapshot snapshot)
        {
            snapshot.HasOtherItems |= HasOtherKeys(embed, EmbedOrder);
            ReadField(embed, "title", Title, snapshot);
            ReadField(embed, "description", Description, snapshot);
            WebhookJsonNode footer = Get(embed, "footer");
            if (footer != null && footer.Kind != WebhookJsonKind.Object)
            {
                snapshot.HasOtherItems = true;
                return;
            }
            snapshot.HasOtherItems |= HasOtherKeys(footer, FooterOrder);
            ReadField(footer, "text", Footer, snapshot);
        }

        private static void ReadField(WebhookJsonNode parent, string key, int field, WebhookSimpleSnapshot snapshot)
        {
            WebhookJsonNode node = Get(parent, key);
            snapshot.Editable[field] = node == null || node.Kind == WebhookJsonKind.String;
            if (node != null && node.Kind == WebhookJsonKind.String)
            {
                snapshot.Values[field] = node.Text;
            }
        }

        private static bool HasOtherKeys(WebhookJsonNode node, string[] order)
        {
            if (node == null)
            {
                return false;
            }
            foreach (string key in node.Keys)
            {
                if (Array.IndexOf(order, key) < 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static WebhookJsonNode Get(WebhookJsonNode parent, string key)
        {
            int index = parent == null ? -1 : parent.IndexOf(key);
            return index < 0 ? null : parent.Children[index];
        }

        internal static bool TryApply(string template, int field, string value, out string result)
        {
            if (field < 0 || field >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(field));
            }
            result = null;
            if (!WebhookJson.TryParse(template, out WebhookJsonNode root, out _) || !ReadRoot(root).Editable[field])
            {
                return false;
            }
            if (field <= Content)
            {
                ApplyString(root, RootOrder[field], value, RootOrder);
            }
            else if (!string.IsNullOrEmpty(value))
            {
                WebhookJsonNode embeds = Get(root, "embeds");
                if (embeds == null)
                {
                    embeds = WebhookJsonNode.CreateArray();
                    Set(root, "embeds", embeds, RootOrder);
                }
                if (embeds.Children.Count == 0)
                {
                    WebhookJsonNode created = WebhookJsonNode.CreateObject();
                    Set(created, "color", WebhookJsonNode.CreateRaw("{color}"), EmbedOrder);
                    embeds.Children.Add(created);
                }
                WebhookJsonNode embed = embeds.Children[0];
                if (field == Footer)
                {
                    WebhookJsonNode footer = Get(embed, "footer");
                    if (footer == null)
                    {
                        footer = WebhookJsonNode.CreateObject();
                        Set(embed, "footer", footer, EmbedOrder);
                    }
                    ApplyString(footer, "text", value, FooterOrder);
                }
                else
                {
                    ApplyString(embed, field == Title ? "title" : "description", value, EmbedOrder);
                }
            }
            else
            {
                RemoveEmbedField(root, field);
            }
            result = WebhookJson.Serialize(root);
            return true;
        }

        private static void RemoveEmbedField(WebhookJsonNode root, int field)
        {
            WebhookJsonNode embeds = Get(root, "embeds");
            if (embeds == null || embeds.Children.Count == 0)
            {
                return;
            }
            if (embeds.Children.Count > 0)
            {
                WebhookJsonNode embed = embeds.Children[0];
                if (field == Footer)
                {
                    WebhookJsonNode footer = Get(embed, "footer");
                    if (footer != null)
                    {
                        Remove(footer, "text");
                        if (footer.Keys.Count == 0)
                        {
                            Remove(embed, "footer");
                        }
                    }
                }
                else
                {
                    Remove(embed, field == Title ? "title" : "description");
                }
                if (embed.Keys.Count == 0 || (embed.Keys.Count == 1 && embed.Keys[0] == "color"))
                {
                    embeds.Children.RemoveAt(0);
                }
            }
            if (embeds.Children.Count == 0)
            {
                Remove(root, "embeds");
            }
        }

        private static void ApplyString(WebhookJsonNode parent, string key, string value, string[] order)
        {
            if (string.IsNullOrEmpty(value))
            {
                Remove(parent, key);
            }
            else
            {
                Set(parent, key, WebhookJsonNode.CreateString(value), order);
            }
        }

        private static void Remove(WebhookJsonNode parent, string key)
        {
            int index = parent.IndexOf(key);
            if (index >= 0)
            {
                parent.Keys.RemoveAt(index);
                parent.Children.RemoveAt(index);
            }
        }

        private static void Set(WebhookJsonNode parent, string key, WebhookJsonNode value, string[] order)
        {
            int index = parent.IndexOf(key);
            if (index >= 0)
            {
                parent.Children[index] = value;
                return;
            }
            int rank = Array.IndexOf(order, key);
            index = parent.Keys.Count;
            for (int i = 0; i < parent.Keys.Count; i++)
            {
                if (Array.IndexOf(order, parent.Keys[i]) > rank)
                {
                    index = i;
                    break;
                }
            }
            parent.Keys.Insert(index, key);
            parent.Children.Insert(index, value);
        }
    }
}
