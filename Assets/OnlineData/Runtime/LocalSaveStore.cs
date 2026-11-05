using System;
using System.IO;
using UnityEngine;

namespace MathFun.OnlineData
{
    /// <summary>
    /// Reads and writes the local save files under Application.persistentDataPath/OnlineData.
    /// A save is written to a temporary file first and then swapped in, with a ".bak" copy of the
    /// previous good file, so a crash while saving cannot destroy the child's progress.
    /// Files: guest.json, account_&lt;name&gt;.json and active_owner.txt (which workspace was open last).
    /// </summary>
    public sealed class LocalSaveStore
    {
        private readonly string folder;

        public LocalSaveStore() : this(Path.Combine(Application.persistentDataPath, "OnlineData")) { }

        /// <summary>Tests pass a temporary folder here.</summary>
        public LocalSaveStore(string folder)
        {
            this.folder = folder;
            Directory.CreateDirectory(folder);
        }

        public const string GuestOwnerKey = "guest";

        public static string AccountOwnerKey(string username)
        {
            return "account:" + username.ToLowerInvariant();
        }

        /// <summary>Loads a workspace. Falls back to the .bak copy, then to a new empty document.</summary>
        public SaveDocument Load(string ownerKey)
        {
            string path = PathFor(ownerKey);
            SaveDocument document = TryRead(path) ?? TryRead(path + ".bak");
            if (document == null)
            {
                document = new SaveDocument { ownerKey = ownerKey };
            }
            if (string.IsNullOrEmpty(document.sourceInstallId))
            {
                document.sourceInstallId = ReadOrCreateInstallId();
            }
            return document;
        }

        /// <summary>Saves a workspace. Returns false (and logs) if the disk write failed; the old file stays intact.</summary>
        public bool Save(SaveDocument document)
        {
            string path = PathFor(document.ownerKey);
            string temp = path + ".tmp";
            try
            {
                File.WriteAllText(temp, JsonUtility.ToJson(document));
                if (File.Exists(path))
                {
                    File.Replace(temp, path, path + ".bak");
                }
                else
                {
                    File.Move(temp, path);
                }
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("MathFun: could not save progress: " + exception.GetType().Name);
                return false;
            }
        }

        public bool Exists(string ownerKey)
        {
            return File.Exists(PathFor(ownerKey)) || File.Exists(PathFor(ownerKey) + ".bak");
        }

        public void Delete(string ownerKey)
        {
            string path = PathFor(ownerKey);
            foreach (string file in new[] { path, path + ".bak", path + ".tmp" })
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        public string LoadActiveOwnerKey()
        {
            string path = Path.Combine(folder, "active_owner.txt");
            return File.Exists(path) ? File.ReadAllText(path).Trim() : GuestOwnerKey;
        }

        public void SaveActiveOwnerKey(string ownerKey)
        {
            File.WriteAllText(Path.Combine(folder, "active_owner.txt"), ownerKey);
        }

        /// <summary>Writes a one-off copy of arbitrary text (used for the legacy PlayerPrefs backup).</summary>
        public string WriteBackup(string fileName, string content)
        {
            string path = Path.Combine(folder, fileName);
            File.WriteAllText(path, content);
            return path;
        }

        private string PathFor(string ownerKey)
        {
            // Only letters, digits and a few symbols are kept so a username cannot escape the folder.
            char[] safe = ownerKey.ToCharArray();
            for (int i = 0; i < safe.Length; i++)
            {
                if (!char.IsLetterOrDigit(safe[i]) && safe[i] != '_' && safe[i] != '-' && safe[i] != '.') safe[i] = '_';
            }
            return Path.Combine(folder, new string(safe) + ".json");
        }

        private static SaveDocument TryRead(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                SaveDocument document = JsonUtility.FromJson<SaveDocument>(File.ReadAllText(path));
                return document != null && document.version <= SaveDocument.CurrentVersion ? document : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>One id per installation, shared by every workspace on this device.</summary>
        private string ReadOrCreateInstallId()
        {
            string path = Path.Combine(folder, "install_id.txt");
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path).Trim();
                if (existing.Length == 36) return existing;
            }
            string created = Guid.NewGuid().ToString();
            File.WriteAllText(path, created);
            return created;
        }
    }
}
