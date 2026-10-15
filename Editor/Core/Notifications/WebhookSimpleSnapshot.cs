namespace Poyo.MarshmallowJar.Editor
{
    internal sealed class WebhookSimpleSnapshot
    {
        internal readonly string[] Values = new string[WebhookSimpleFields.Count];
        internal readonly bool[] Editable = new bool[WebhookSimpleFields.Count];
        internal bool HasOtherItems;
        internal bool HasVisibleContent;
        internal string ErrorMessage;

        internal WebhookSimpleSnapshot()
        {
            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = string.Empty;
            }
        }
    }
}
