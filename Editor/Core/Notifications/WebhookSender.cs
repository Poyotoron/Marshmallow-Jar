using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class WebhookSender
    {
        internal static void Send(string url, string template, string title, string message, bool success, bool logSuccess)
        {
            if (string.IsNullOrEmpty(url) ||
                (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            UnityWebRequest request = null;
            try
            {
                string body = Expand(template, title, message, success);
                request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 15;
                UnityWebRequest pending = request;
                request.SendWebRequest().completed += operation => OnCompleted(pending, logSuccess);
            }
            catch (Exception)
            {
                request?.Dispose();
                // NOTE: URL は知っていれば誰でも投稿できる秘密情報のため、送信開始時の例外本文は表示しない。
                Debug.LogWarning(MarshmallowJarInfo.LogPrefix + "Webhook の送信を開始できませんでした。URL と本文の設定を確認してください。");
            }
        }

        private static void OnCompleted(UnityWebRequest request, bool logSuccess)
        {
            try
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    string error = request.error ?? string.Empty;
                    // NOTE: 通信エラーに送信先が含まれても、秘密の URL はログに残さない。
                    if (!string.IsNullOrEmpty(request.url))
                    {
                        error = error.Replace(request.url, "（送信先）");
                    }
                    Debug.LogWarning(MarshmallowJarInfo.LogPrefix + "Webhook の送信に失敗しました（" + request.responseCode + "）: " + error);
                }
                else if (logSuccess)
                {
                    Debug.Log(MarshmallowJarInfo.LogPrefix + "Webhook にテスト通知を送りました。");
                }
            }
            finally
            {
                request.Dispose();
            }
        }

        internal static string Expand(string template, string title, string message, bool success)
        {
            template = template ?? string.Empty;
            var builder = new StringBuilder(template.Length + 128);
            // NOTE: 置き換えた本文の中にある {名前} を再び展開しないため、元のテンプレートだけを一度走査する。
            for (int i = 0; i < template.Length; i++)
            {
                if (template[i] == '{')
                {
                    int end = i + 1;
                    while (end < template.Length && template[end] != '}' && template[end] != '{')
                    {
                        end++;
                    }
                    if (end < template.Length && template[end] == '}')
                    {
                        string key = template.Substring(i + 1, end - i - 1);
                        string value = null;
                        switch (key)
                        {
                            case "title":
                                value = title ?? string.Empty;
                                break;
                            case "message":
                                value = message ?? string.Empty;
                                break;
                            case "result":
                                value = success ? "成功" : "失敗";
                                break;
                            case "project":
                                value = Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
                                break;
                            case "time":
                                value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                break;
                            case "color":
                                builder.Append(success ? "5763719" : "15548997");
                                i = end;
                                continue;
                        }
                        if (value != null)
                        {
                            AppendJsonEscaped(builder, value);
                            i = end;
                            continue;
                        }
                    }
                }
                builder.Append(template[i]);
            }
            return builder.ToString();
        }

        internal static void AppendJsonEscaped(StringBuilder builder, string value)
        {
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (c < 0x20)
                        {
                            builder.Append("\\u");
                            builder.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(c);
                        }
                        break;
                }
            }
        }
    }
}
