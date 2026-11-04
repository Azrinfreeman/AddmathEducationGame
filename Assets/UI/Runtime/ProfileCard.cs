using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class ProfileCard : MonoBehaviour
    {
        public TMP_Text playerName;
        public TMP_Text age;
        public AvatarPortrait portrait;
        public Button play;
        public Button changeAvatar;
        public AvatarPickerDialog avatarDialog;
        private AvatarPickerDialog pending;

        public void Bind(int id, AccountController accounts)
        {
            playerName.text = PlayerPrefs.GetString("PlayerName_" + id, "Player " + id);
            age.text = PlayerPrefs.GetInt("PlayerAge_" + id) + (PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? " years" : " tahun");
            portrait.Show(portrait.catalog.Read(id));
            play.onClick.AddListener(() => accounts.GoToMainMenu(id));
            changeAvatar.onClick.AddListener(() =>
            {
                if (pending) return;
                pending = Instantiate(avatarDialog);
                pending.Open(id, () => portrait.Show(portrait.catalog.Read(id)), ModalInputOwner.ForScene(gameObject.scene));
            });
        }
    }
}
