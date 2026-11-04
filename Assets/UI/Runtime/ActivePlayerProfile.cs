using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ActivePlayerProfile : MonoBehaviour
    {
        public AvatarPortrait portrait;
        public TMP_Text playerName;
        public AvatarPickerDialog dialogPrefab;
        private AvatarPickerDialog pending;

        private void Start()
        {
            int id = PlayerPrefs.GetInt("PlayerID", 0);
            playerName.text = PlayerPrefs.GetString("PlayerName", "");
            portrait.Show(portrait.catalog.Read(id));
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (pending || id <= 0) return;
                pending = Instantiate(dialogPrefab);
                pending.Open(id, () => portrait.Show(portrait.catalog.Read(id)), ModalInputOwner.ForScene(gameObject.scene));
            });
        }
    }
}
