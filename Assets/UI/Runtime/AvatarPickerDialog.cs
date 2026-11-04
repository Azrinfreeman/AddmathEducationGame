using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class AvatarPickerDialog : MonoBehaviour
    {
        public AvatarPicker picker;
        public Button confirm;
        public Button cancel;
        private GameObject previousFocus;

        private ModalInputScope scope;
        public void Open(int playerId, Action saved, ModalInputOwner owner = null)
        {
            previousFocus = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            picker.Select(picker.catalog.Read(playerId));
            confirm.onClick.AddListener(() =>
            {
                PlayerPrefs.SetString("PlayerAvatar_" + playerId, picker.SelectedId);
                PlayerPrefs.Save();
                saved?.Invoke();
                Close();
            });
            cancel.onClick.AddListener(Close);
            if (owner)
            {
                scope = gameObject.AddComponent<ModalInputScope>(); scope.firstControl = picker.choices[0].button;
                scope.closeRequested = Close; scope.Bind(owner);
            }
            else if (EventSystem.current) EventSystem.current.SetSelectedGameObject(picker.choices[0].button.gameObject);
        }

        private void Close()
        {
            if (scope && scope.owner) scope.owner.Pop(scope);
            else if (EventSystem.current && previousFocus) EventSystem.current.SetSelectedGameObject(previousFocus);
            Destroy(gameObject);
        }
    }
}
