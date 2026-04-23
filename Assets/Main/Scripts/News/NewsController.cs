using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NarrativeGP.News
{
    public class NewsController : MonoBehaviour
    {
        [System.Serializable]
        private class NewsRuntimeState
        {
            public string newsId;
            public int currentVersion;
            public bool hasBeenOpened;
        }

        [Header("Dependencies")]
        [SerializeField] private GameState gameState;

        [Header("Data")]
        [SerializeField] private List<NewsData> newsItems = new();

        [Header("List UI")]
        [SerializeField] private Transform newsListContent;
        [SerializeField] private NewsListItemView newsItemPrefab;

        [Header("Detail UI")]
        [SerializeField] private TMP_Text displayTitleText;
        [SerializeField] private Image articleImage;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button readingAreaButton;
        [SerializeField] private Color bodyHighlightColor = new(1f, 0.93f, 0.35f, 0.75f);

        [Header("Runtime State")]
        [SerializeField] private List<NewsRuntimeState> runtimeStates = new();

        private readonly Dictionary<string, NewsData> newsLookup = new();
        private readonly Dictionary<string, NewsRuntimeState> runtimeLookup = new();
        private readonly List<NewsData> visibleNewsItems = new();
        private readonly List<NewsListItemView> spawnedItemViews = new();
        private string selectedNewsId;

        private void Reset()
        {
            gameState = FindFirstObjectByType<GameState>();
        }

        private void Awake()
        {
            BuildLookups();
            EnsureRuntimeStatesExist();
        }

        private void OnEnable()
        {
            if (gameState != null)
            {
                gameState.StateChanged += HandleGameStateChanged;
            }

            RefreshView();
        }

        private void OnDisable()
        {
            if (gameState != null)
            {
                gameState.StateChanged -= HandleGameStateChanged;
            }
        }

        public void SelectNews(string newsId)
        {
            if (string.IsNullOrWhiteSpace(newsId) || !visibleNewsItems.Any(news => news.id == newsId))
            {
                return;
            }

            selectedNewsId = newsId;
            MarkSelectedNewsOpenedByUser();
            RefreshView();
        }

        public void AdvanceReading()
        {
            if (!TryGetSelectedNews(out NewsData selectedNews))
            {
                return;
            }

            NewsRuntimeState runtimeState = GetRuntimeState(selectedNews.id);
            int maxVersion = GetMaxVersion(selectedNews);
            if (runtimeState.currentVersion >= maxVersion)
            {
                return;
            }

            runtimeState.currentVersion++;
            RefreshView();
        }

        [ContextMenu("Refresh News")]
        public void RefreshView()
        {
            BuildVisibleNewsList();
            EnsureSelectedNews();
            RebuildListItems();
            RefreshListItems();
            RefreshDetailPanel();
            RefreshReadingAreaButton();
        }

        private void BuildLookups()
        {
            newsLookup.Clear();
            runtimeLookup.Clear();

            foreach (NewsData newsData in newsItems)
            {
                if (newsData == null || string.IsNullOrWhiteSpace(newsData.id))
                {
                    continue;
                }

                newsLookup[newsData.id] = newsData;
            }

            foreach (NewsRuntimeState runtimeState in runtimeStates)
            {
                if (runtimeState == null || string.IsNullOrWhiteSpace(runtimeState.newsId))
                {
                    continue;
                }

                runtimeLookup[runtimeState.newsId] = runtimeState;
            }
        }

        private void EnsureRuntimeStatesExist()
        {
            foreach (NewsData newsData in newsItems)
            {
                if (newsData == null || string.IsNullOrWhiteSpace(newsData.id))
                {
                    continue;
                }

                if (runtimeLookup.ContainsKey(newsData.id))
                {
                    continue;
                }

                var runtimeState = new NewsRuntimeState
                {
                    newsId = newsData.id,
                    currentVersion = 0
                };

                runtimeStates.Add(runtimeState);
                runtimeLookup.Add(newsData.id, runtimeState);
            }
        }

        private void BuildVisibleNewsList()
        {
            visibleNewsItems.Clear();

            int currentDay = gameState != null ? Mathf.Max(1, gameState.CurrentDay) : 1;
            foreach (NewsData newsData in newsItems)
            {
                if (newsData == null || string.IsNullOrWhiteSpace(newsData.id))
                {
                    continue;
                }

                if (newsData.arrivalDay <= currentDay)
                {
                    visibleNewsItems.Add(newsData);
                }
            }

            visibleNewsItems.Sort(CompareNewsItems);
        }

        private void EnsureSelectedNews()
        {
            if (!string.IsNullOrWhiteSpace(selectedNewsId) && visibleNewsItems.Any(news => news.id == selectedNewsId))
            {
                return;
            }

            foreach (NewsData newsData in visibleNewsItems)
            {
                if (newsData != null && !string.IsNullOrWhiteSpace(newsData.id))
                {
                    selectedNewsId = newsData.id;
                    return;
                }
            }

            selectedNewsId = string.Empty;
        }

        private void RebuildListItems()
        {
            for (int i = spawnedItemViews.Count - 1; i >= 0; i--)
            {
                if (spawnedItemViews[i] != null)
                {
                    Destroy(spawnedItemViews[i].gameObject);
                }
            }

            spawnedItemViews.Clear();

            if (newsListContent == null || newsItemPrefab == null)
            {
                return;
            }

            foreach (NewsData newsData in visibleNewsItems)
            {
                if (newsData == null || string.IsNullOrWhiteSpace(newsData.id))
                {
                    continue;
                }

                NewsListItemView itemView = Instantiate(newsItemPrefab, newsListContent);
                itemView.Initialize(this);
                itemView.Bind(newsData.id, newsData.listTitle);
                spawnedItemViews.Add(itemView);
            }
        }

        private void RefreshListItems()
        {
            foreach (NewsListItemView itemView in spawnedItemViews)
            {
                if (itemView == null)
                {
                    continue;
                }

                bool isSelected = itemView.NewsId == selectedNewsId;
                bool isCompleted = IsNewsCompleted(itemView.NewsId);
                itemView.SetState(isCompleted, isSelected);
            }
        }

        private void RefreshDetailPanel()
        {
            if (!TryGetSelectedNews(out NewsData selectedNews))
            {
                ApplyBlankDetail();
                return;
            }

            SetText(displayTitleText, string.IsNullOrWhiteSpace(selectedNews.displayTitle) ? selectedNews.listTitle : selectedNews.displayTitle);
            if (articleImage != null)
            {
                articleImage.sprite = selectedNews.image;
                articleImage.enabled = selectedNews.image != null;
            }

            int currentVersion = GetRuntimeState(selectedNews.id).currentVersion;
            string versionText = GetBodyVersion(selectedNews, currentVersion);
            SetText(bodyText, ApplyHighlightMarkup(versionText));
        }

        private void RefreshReadingAreaButton()
        {
            if (readingAreaButton == null)
            {
                return;
            }

            if (!TryGetSelectedNews(out NewsData selectedNews))
            {
                readingAreaButton.interactable = false;
                return;
            }

            NewsRuntimeState runtimeState = GetRuntimeState(selectedNews.id);
            readingAreaButton.interactable = runtimeState.currentVersion < GetMaxVersion(selectedNews);
        }

        private void MarkSelectedNewsOpenedByUser()
        {
            if (!TryGetSelectedNews(out NewsData selectedNews))
            {
                return;
            }

            GetRuntimeState(selectedNews.id).hasBeenOpened = true;
        }

        private bool IsNewsCompleted(string newsId)
        {
            if (!newsLookup.TryGetValue(newsId, out NewsData newsData))
            {
                return false;
            }

            NewsRuntimeState runtimeState = GetRuntimeState(newsId);
            int maxVersion = GetMaxVersion(newsData);

            if (maxVersion == 0)
            {
                return runtimeState.hasBeenOpened;
            }

            return runtimeState.currentVersion >= maxVersion;
        }

        private NewsRuntimeState GetRuntimeState(string newsId)
        {
            if (!runtimeLookup.TryGetValue(newsId, out NewsRuntimeState runtimeState))
            {
                runtimeState = new NewsRuntimeState
                {
                    newsId = newsId,
                    currentVersion = 0
                };

                runtimeStates.Add(runtimeState);
                runtimeLookup.Add(newsId, runtimeState);
            }

            return runtimeState;
        }

        private static int GetMaxVersion(NewsData newsData)
        {
            if (newsData == null || newsData.bodyVersions == null || newsData.bodyVersions.Count == 0)
            {
                return 0;
            }

            return newsData.bodyVersions.Count - 1;
        }

        private static string GetBodyVersion(NewsData newsData, int version)
        {
            if (newsData == null || newsData.bodyVersions == null || newsData.bodyVersions.Count == 0)
            {
                return string.Empty;
            }

            int clampedVersion = Mathf.Clamp(version, 0, newsData.bodyVersions.Count - 1);
            NewsData.BodyVersionEntry entry = newsData.bodyVersions[clampedVersion];
            return entry != null ? entry.text ?? string.Empty : string.Empty;
        }

        private string ApplyHighlightMarkup(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText))
            {
                return string.Empty;
            }

            string colorMarkup = ColorUtility.ToHtmlStringRGBA(bodyHighlightColor);
            return sourceText.Replace("<mark>", $"<mark=#{colorMarkup}>");
        }

        private bool TryGetSelectedNews(out NewsData selectedNews)
        {
            if (!string.IsNullOrWhiteSpace(selectedNewsId) && newsLookup.TryGetValue(selectedNewsId, out selectedNews) && visibleNewsItems.Any(item => item.id == selectedNewsId))
            {
                return true;
            }

            selectedNews = null;
            return false;
        }

        private void ApplyBlankDetail()
        {
            SetText(displayTitleText, string.Empty);
            SetText(bodyText, string.Empty);

            if (articleImage != null)
            {
                articleImage.sprite = null;
                articleImage.enabled = false;
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private void HandleGameStateChanged()
        {
            RefreshView();
        }

        private static int CompareNewsItems(NewsData left, NewsData right)
        {
            int arrivalComparison = right.arrivalDay.CompareTo(left.arrivalDay);
            if (arrivalComparison != 0)
            {
                return arrivalComparison;
            }

            int sortOrderComparison = left.sortOrder.CompareTo(right.sortOrder);
            if (sortOrderComparison != 0)
            {
                return sortOrderComparison;
            }

            return string.CompareOrdinal(left.id, right.id);
        }
    }
}
