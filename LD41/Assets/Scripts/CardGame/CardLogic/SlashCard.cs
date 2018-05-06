using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CardGame.CardLogic
{
    public class SlashCard : ICard
    {
        private PoolInstance _poolInstance;
        private MainCharacter _mainCharacter;
        public SlashCard(PoolInstance poolInstance)
        {
            _poolInstance = poolInstance;
            Toolbox.TryGetMainCharacter(out _mainCharacter);
        }

        public IEnumerator<MakineryGear> DoLogic(MonoBehaviour unity)
        {
            var gameObject = Toolbox.Instance.Pool.Retrieve(_poolInstance);
            gameObject.SetActive(true);
            gameObject.transform.position = _mainCharacter.CharacterTransform.position + (Vector3)_mainCharacter.GetDirection() * 0.5f;
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 
                Angle(_mainCharacter.GetDirection()) - 270 * (_mainCharacter.GetDirection().x != 0 ? -1 : 1));

            var realPos = _mainCharacter.CharacterTransform.position + (Vector3)_mainCharacter.GetDirection();
            var realPosGrid = Toolbox.Instance.LevelGrid.Vector2ToGrid(realPos);
            
            Toolbox.Instance.StartCoroutine(Animate(gameObject,()=>
            {
                Toolbox.Instance.Pool.Release(_poolInstance, gameObject);
            }));

            Makinery flashAndAttack = new Makinery(50);
            flashAndAttack.AddRoutine(() => FlashAndAttack(realPosGrid, _mainCharacter));

            yield return new InnerMakinery(flashAndAttack, Toolbox.Instance.MainMakina);
        }

        IEnumerator<MakineryGear> FlashAndAttack(int[] realPosGrid, MainCharacter mainCharacter)
        {
            var pushDirection = mainCharacter.GetDirection();
            Toolbox.Instance.LevelGrid.FlashTile(realPosGrid, 0.3f, Colors.MidRed);
            yield return new WaitForSecondsGear(0.3f);
            Toolbox.Instance.LevelGrid.FlashTile(realPosGrid, 0.2f, Color.white);

            Toolbox.Instance.LevelGrid.TriggerGridEvent(LevelGrid.GridEvents.HeroAttack,
                mainCharacter.GridObject, realPosGrid, new Dictionary<string, object>()
                {
                        { "Damage", 1 * mainCharacter.Stats.Attack},
                        { "Push", pushDirection }
                });
            yield return new WaitForSecondsGear(0.2f);
            Toolbox.Instance.LevelGrid.FlashTile(realPosGrid, 0.2f, Colors.MidRed);            
        }

        IEnumerator Animate(GameObject obj, Action onEnd)
        {
            var sprRenderer = obj.GetComponent<SpriteRenderer>();
            if (sprRenderer == null) yield break;

            float lumi = 0.65f;
            float hue = 50;

            float time = 0f;
            while (time <= 1f)
            {
                float lumiLerp = QuintEaseInOut(time, 0, lumi, 1f);
                float hueLerp = QuintEaseInOut(time, 0, hue, 1f);
                sprRenderer.material.SetFloat("_Luminance", lumiLerp);
                sprRenderer.material.SetFloat("_Hue", hueLerp);
                time += Time.deltaTime * 6f;
                yield return new WaitForEndOfFrame();
            }

            sprRenderer.material.SetFloat("_Luminance", lumi);
            sprRenderer.material.SetFloat("_Hue", hue);

            time = 0f;
            while (time <= 1f)
            {
                float lumiLerp = Mathf.Lerp(lumi, 0, time * time);
                float hueLerp = Mathf.Lerp(hue, 0, time * time);
                sprRenderer.material.SetFloat("_Luminance", lumiLerp);
                sprRenderer.material.SetFloat("_Hue", hueLerp);
                time += Time.deltaTime * 3f;
                yield return new WaitForEndOfFrame();
            }

            sprRenderer.material.SetFloat("_Luminance", 0f);
            sprRenderer.material.SetFloat("_Hue", 0);
            sprRenderer.material.SetFloat("_Saturation", 0);

            onEnd();

            yield break;
        }

        float QuintEaseInOut(float t, float b, float c, float d)
        {
            if ((t /= d / 2) < 1)
                return c / 2 * t * t * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
        }

        float Angle(Vector2 p_vector2)
        {
            if (p_vector2.x < 0)
            {
                return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
            }
            else
            {
                return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
            }
        }

        public bool CanUse()
        {
            return !_mainCharacter.IsIncapacitated;
        }
    }
}
