using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelTop : MonoBehaviour
    {
        public static PanelTop Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public TextMeshProUGUI textCoinsCounter;
        [Required]
        public Image imageCoins;
        [Required]
        public GameObject coinViewTemplate;
        [Required]
        public PanelTopStatIndicator indicatorAttack;
        [Required]
        public PanelTopStatIndicator indicatorDefence;
        [Required]
        public PanelTopStatIndicator indicatorMagic;
        //public AudioSimpleData audioCoinEndMove;
        //public AudioSimpleData audioCoinStartMove;
        public float scoresAnimTime;
        public float coinsAddIntense;
        public float coinsAddFlySpeed;

        private int curCoins;
        private Tweener tweenerScoresCounter;

        public PanelTop() => Default = this;
        private void Awake()
        {
            ProcessorObserver.Default.Add(
                () => DataGameMain.Default.Coins, coins => UpdateViewCoinsCounter(), true);

            UpdateViewCoinsCounter();

            ProcessorObserver.Default.Add(
                () => PlayerController.Current.character.lvl3StatsResultSum.main.attack,
                attack => indicatorAttack.UpdateView(Convert.ToInt32(attack)), true);
            indicatorAttack.OnIncrease += () => 
            PlayerController.Current.character.lvl0StatsCharacter.main.attack += DataGameMain.Default.statsStepPerSkillPoint;

            ProcessorObserver.Default.Add(
                () => PlayerController.Current.character.lvl3StatsResultSum.main.defence,
                defence => indicatorDefence.UpdateView(Convert.ToInt32(defence)), true);
            indicatorDefence.OnIncrease += () =>
            PlayerController.Current.character.lvl0StatsCharacter.main.defence += DataGameMain.Default.statsStepPerSkillPoint;

            ProcessorObserver.Default.Add(
                () => PlayerController.Current.character.lvl3StatsResultSum.main.magic,
                magic => indicatorMagic.UpdateView(Convert.ToInt32(magic)), true);
            indicatorMagic.OnIncrease += () =>
            PlayerController.Current.character.lvl0StatsCharacter.main.magic += DataGameMain.Default.statsStepPerSkillPoint;
        }
        private void UpdateViewCoinsCounter()
        {
            if (tweenerScoresCounter != null)
            {
                imageCoins.transform.DOKill();
                tweenerScoresCounter.Kill();
                tweenerScoresCounter = null;
            }

            if (imageCoins.transform.localScale.Equals(Vector3.one))
                imageCoins.transform.DOScale(1.25f, 0.25f).onComplete = () =>
                {
                    imageCoins.transform.DOScale(1f, 0.25f);
                };
            else
                imageCoins.transform.DOScale(1f, 0.25f);
            tweenerScoresCounter = DOTween.To(() => curCoins, x => curCoins = x, DataGameMain.Default.Coins, scoresAnimTime)
                .OnUpdate(() =>
                {
                    textCoinsCounter.text = curCoins.ToString();
                });
        }
        public void AddCoins(int amount, Vector3 position, Action onComplete = default)
        {
            StartCoroutine(_AddCoins(amount, position, onComplete));
        }
        private IEnumerator _AddCoins(int amount, Vector3 position, Action onComplete = default)
        {
            for (int i = 0; i < 10 && i < amount; i++)
            {
                yield return new WaitForSeconds(1f / coinsAddIntense);
                GameObject coin = Instantiate(coinViewTemplate, UIManager.Default.mainCanvas.transform);
                //if (amount >= 10)
                //    diamond.transform.localScale += Vector3.one * 0.25f;
                //if (amount >= 100)
                //    diamond.transform.localScale += Vector3.one * 0.25f;
                coin.SetActive(true);
                coin.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
                coin.transform.SetAsLastSibling();
                coin.transform.position = position;

                bool isFirst = i == 0;
                bool isLast = i + 1 == 10 || i + 1 == amount;

                //audioCoinStartMove.Play(ProcessorSoundPool.PoolLevel.GameLevel);
                Tweener tweener = coin.transform.DOMove(imageCoins.transform.position, 1f / coinsAddFlySpeed);
                tweener.onComplete = () =>
                {
                    //audioCoinEndMove.Play(ProcessorSoundPool.PoolLevel.GameLevel);
                    Destroy(coin);
                    if (isFirst)
                    {
                        DataGameMain.Default.Coins += amount;
                    }
                    if (isLast)
                    {
                        if (onComplete != null)
                            onComplete.Invoke();
                    }
                };
            }
            yield return null;
        }
    }
}
