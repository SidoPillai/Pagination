using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Esri.ArcGISRuntime.Portal;
using Microsoft.Maui.Controls;

namespace Pagination.ViewModels
{
    public class ItemsViewModel : BindableObject
    {
        private int _startIndex = 1;
        private bool _isLoading;
        private ArcGISPortal _portal;

        public ObservableCollection<PortalItem> Items { get; set; } = new ObservableCollection<PortalItem>();

        //public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();

        public ICommand LoadMoreItemsCommand { get; }

        public ItemsViewModel()
        {
            InitializePortalAsync();
            LoadMoreItemsCommand = new Command(async () => await LoadMoreItems());
        }

        private async void InitializePortalAsync()
        {
            _portal = await ArcGISPortal.CreateAsync();
            var portalInfo = _portal.PortalInfo;

            Trace.WriteLine(string.Format("Access: {0}", portalInfo.Access.ToString()));
            Trace.WriteLine(string.Format("Name: {0}", portalInfo.PortalName));
            Trace.WriteLine(string.Format("Mode: {0}", portalInfo.PortalMode.ToString()));

            LoadInitialItems();
        }

        private async void LoadInitialItems()
        {
            Items.Clear();
            _startIndex = 1;
            var results = await Search();
            var nextStartIndex = (int)results["NextStart"];
            Trace.WriteLine(string.Format("Next Start {0}", _startIndex));
            foreach (var item in (System.Collections.Generic.IEnumerable<PortalItem>)results["items"])
            {
                Items.Add(item);
            }
        }

        private async Task LoadMoreItems()
        {
            if (_startIndex == -1)
            {
                Trace.WriteLine("End of the list...");
                return;
            }

            if (_isLoading)
            {
                Trace.WriteLine("Already loading..");
                return;
            }

            Trace.WriteLine("Loading more items...");

            _isLoading = true;

            var results = await Search();
            var nextStartIndex = (int)results["NextStart"];
            foreach (var item in (System.Collections.Generic.IEnumerable<PortalItem>)results["items"])
            {
                Items.Add(item);
            }
            _startIndex = nextStartIndex;
            _isLoading = false;
        }

        public async Task<Dictionary<string, object>> Search()
        {
            Trace.WriteLine(string.Format("Searching for nextStart {0}", _startIndex));
            string query = "((type: \"StoryMap\") AND typekeywords:\"storymapbriefing\")";
            var queryParams = new PortalQueryParameters(query)
            {
                CanSearchPublic = true,
                StartIndex = _startIndex,
                SortField = "modified",
                SortOrder = PortalQuerySortOrder.Ascending,
                Limit = 50,

            };

            var result = await _portal.FindItemsAsync(queryParams);
            if (result == null)
            {
                Trace.WriteLine("No results found.");
                return new Dictionary<string, object>()
                {
                    ["total"] = 0,
                    ["items"] = new List<object>(),
                    ["NextStart"] = 0
                };
            }

            var res = result.Results.ToList();
            Trace.WriteLine($"Total results: {res.Count}");
            return new Dictionary<string, object>()
            {
                ["total"] = result.TotalResultsCount,
                ["items"] = result.Results.ToList(),
                ["NextStart"] = result.NextQueryParameters?.StartIndex ?? -1
            };
        }
    }
}