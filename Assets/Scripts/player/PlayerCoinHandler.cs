using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerCoinHandler : MonoBehaviour
    {
        public Text ui; // 用于显示分数的UI文本
        public Text highestScoreUI; // 用于显示最高分数的UI文本
        public int score = 0; // 当前分数
        private int highestScore = 0; // 历史最高分数

        // Start is called before the first frame update
        private void Start()
        {
            // 从玩家数据加载最高分数（如果需要持久化存储，可以用 PlayerPrefs 或其他方式）
            highestScore = PlayerPrefs.GetInt("HighestScore", 0); // 从存储中加载最高分，默认为0
            UpdateUI(); // 初始化UI
        }

        // Update is called once per frame
        private void Update()
        {
            // 如果有额外逻辑可以写在这里
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("coin"))
            {
                // 增加当前分数
                score++;

                // 检查是否超过最高分
                if (score > highestScore)
                {
                    highestScore = score; // 更新最高分
                    PlayerPrefs.SetInt("HighestScore", highestScore); // 保存最高分到本地存储
                }

                // 更新UI
                UpdateUI();

                // 销毁金币对象
                Destroy(other.gameObject);
            }
        }

        private void UpdateUI()
        {
            // 更新分数UI
            ui.text = "Score: " + score;

            // 更新最高分UI
            if (highestScoreUI != null)
            {
                highestScoreUI.text = "Highest Score: " + highestScore;
            }
        }
    }
}
