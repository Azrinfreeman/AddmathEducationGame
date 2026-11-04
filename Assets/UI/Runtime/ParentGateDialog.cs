using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class ParentGateDialog : MonoBehaviour
    {
        public TMP_Text question;
        public Button[] answers;
        public Button cancel;
        private Action approved;
        private GameObject previousSelection;
        private ModalInputScope scope;
        public void Open(Action onApproved, ModalInputOwner owner = null)
        {
            var language = question.GetComponent<BilingualLabel>();
            if (language) language.enabled = false;
            approved = onApproved;
            previousSelection = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            int a = UnityEngine.Random.Range(12, 30), b = UnityEngine.Random.Range(12, 30);
            bool english = PlayerPrefs.GetInt("EnglishLang", 0) == 1;
            question.text = (english ? "Grown-ups only\n" : "Untuk ibu bapa\n") + a + " + " + b + " = ?";
            int correct = UnityEngine.Random.Range(0, answers.Length);
            for (int i = 0; i < answers.Length; i++)
            {
                bool right = i == correct;
                int value = a + b + (i - correct) * 3;
                answers[i].GetComponentInChildren<TMP_Text>().text = value.ToString();
                answers[i].onClick.AddListener(() =>
                {
                    if (right) { var callback = approved; Close(); callback?.Invoke(); }
                    else { question.text = english ? "Ask a grown-up to try again." : "Minta ibu bapa cuba lagi."; foreach (var answer in answers) answer.interactable = false; }
                });
            }
            cancel.GetComponentInChildren<TMP_Text>().text = english ? "Back" : "Kembali";
            cancel.onClick.AddListener(Close);
            if (owner)
            {
                scope = gameObject.AddComponent<ModalInputScope>(); scope.firstControl = cancel;
                scope.closeRequested = Close; scope.Bind(owner);
            }
            else if (EventSystem.current) EventSystem.current.SetSelectedGameObject(cancel.gameObject);
        }
        public void Close()
        {
            approved = null;
            if (scope && scope.owner) scope.owner.Pop(scope);
            else if (EventSystem.current) EventSystem.current.SetSelectedGameObject(previousSelection);
            Destroy(gameObject);
        }
    }
}
