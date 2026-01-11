using Newtonsoft.Json;
using System;
using System.Collections;
using System.Net.Http;
using TMPro;
using UnityEngine;

namespace Lavender
{
    internal class GitHubRelease
    {
        public static GitHubRelease? latestFoundRelease = null;

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("prerelease")]
        public bool IsPrerelease { get; set; }

        [JsonIgnore]
        public bool CurrentIsLatest = true;
    }

    internal static class GitHubVersionChecker
    {
        const string url = "https://api.github.com/repos/leonarudo/Lavender/releases/latest";

        static bool AlreadyChecked = false;

        public static IEnumerator CheckLatestVersionCoroutine()
        {
            if(AlreadyChecked)
            {
                LavenderLog.Log("Skiping Version Check!");

                yield return new WaitForSeconds(0.5f);

                EditMainMenuVersionText();
                yield break;
            }

            LavenderLog.Log("Checking for latest GitHub release...");
            AlreadyChecked = true;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"LavenderVersionChecker/{BepinexPlugin.LavenderVersion.ToString()}");

            var task = client.GetStringAsync(url);

            while (!task.IsCompleted)
            {
                yield return null; // wait for next frame
            }

            if (task.IsFaulted)
            {
                LavenderLog.Error($"GitHubVersionChecker: Failed to get latest version: {task.Exception?.Message}");
                EditMainMenuVersionText();
                yield break;
            }

            try
            {
                var json = task.Result;
                var release = JsonConvert.DeserializeObject<GitHubRelease>(json);

                if(release == null)
                {
                    EditMainMenuVersionText();
                    yield break;
                }

                GitHubRelease.latestFoundRelease = release;

                // Version Check
                if (release.TagName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    release.TagName = release.TagName.Substring(1); // remove the leading "v"
                }

                Version gitVersion;
                if (!Version.TryParse(release.TagName, out gitVersion))
                {
                    LavenderLog.Error($"Failed to parse version from tag: {release.TagName}");
                    EditMainMenuVersionText();
                    yield break;
                }

                if (gitVersion > BepinexPlugin.LavenderVersion)
                {
                    if(release.IsPrerelease)
                    {
                        LavenderLog.Log($"A newer preview version is available: {gitVersion} - Currently installed version: {BepinexPlugin.LavenderVersion}");
                        LavenderLog.Log("No need to update yet ;)");
                    }
                    else
                    {
                        LavenderLog.Log($"A newer version is available: {gitVersion} - Currently installed version: {BepinexPlugin.LavenderVersion}");
                        LavenderLog.Log("Please install the newest update to ensure game stability!");

                        GitHubRelease.latestFoundRelease.CurrentIsLatest = false;
                    }
                }
                else if (gitVersion == BepinexPlugin.LavenderVersion)
                {
                    LavenderLog.Log("You are running the latest version.");
                }
                else
                {
                    LavenderLog.Log($"Your running an unreleased dev build! Latest: {gitVersion}, you: {BepinexPlugin.LavenderVersion}");
                }
            }
            catch(Exception ex)
            {
                LavenderLog.Error($"GitHubVersionChecker: Error while parsing release info: {ex.ToString()}");
            }

            EditMainMenuVersionText();
        }

        public static void EditMainMenuVersionText()
        {
            TextMeshProUGUI Versiontext = GameObject.FindObjectOfType<ShowVersion>().Versiontext;

            string lavenderVersionText = $"<br>Lavender v{BepinexPlugin.LavenderVersion}";
            if (GitHubRelease.latestFoundRelease != null && GitHubRelease.latestFoundRelease.TagName != null)
            {
                if (!GitHubRelease.latestFoundRelease.CurrentIsLatest && !GitHubRelease.latestFoundRelease.IsPrerelease)
                {
                    lavenderVersionText = $"<br>Lavender <color=\"red\">v{BepinexPlugin.LavenderVersion}</color><br>Lavender <color=\"green\">v{GitHubRelease.latestFoundRelease.TagName}</color> available!";
                }
                else
                {
                    lavenderVersionText = $"<br>Lavender <color=\"green\">v{BepinexPlugin.LavenderVersion}</color>";
                }
            }

            Versiontext.text = "Obenseuer V. " + Application.version + lavenderVersionText;
        }
    }
}
