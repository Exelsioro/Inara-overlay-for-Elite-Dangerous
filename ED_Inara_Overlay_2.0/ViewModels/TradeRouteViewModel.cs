using InaraTools;

namespace ED_Inara_Overlay_2._0.ViewModels
{
    /// <summary>
    /// View model wrapper for TradeRoute to support WPF data binding
    /// </summary>
    public class TradeRouteViewModel
    {
        private readonly TradeRoute _tradeRoute;

        public TradeRouteViewModel(TradeRoute tradeRoute)
        {
            _tradeRoute = tradeRoute;
        }

// Expose the original TradeRoute properties
        public TradeLeg FirstRoute => _tradeRoute.FirstRoute;
        public CardHeader CardHeader => _tradeRoute.CardHeader;
        public bool IsRoundTrip => _tradeRoute.IsRoundTrip;
        public TradeLeg? SecondRoute => _tradeRoute.SecondRoute;
        public double TotalProfitPerHour => _tradeRoute.TotalProfitPerHour;
        public string LastUpdate => _tradeRoute.LastUpdate;
        public int TotalProfitPerUnit => _tradeRoute.TotalProfitPerUnit;
        public double TotalRouteDistance => _tradeRoute.TotalRouteDistance;

        // Additional properties for UI display
        public string RouteDescription
        {
            get
            {
                var fromStation = CardHeader.FromStation;
                var toStation = CardHeader.ToStation;
                return $"{fromStation.Name} | {fromStation.System} → {toStation.Name} | {toStation.System}";
            }
        }

        public string ProfitDisplay => $"{FirstRoute.ProfitPerUnit:N0} Cr";
        public string DistanceDisplay => $"{TotalRouteDistance:F2} Ly";
        public string BuyPriceDisplay => $"{FirstRoute.BuyCommodity.Price:N0} Cr";
        public string SellPriceDisplay => $"{FirstRoute.SellCommodity.Price:N0} Cr";
    }
}
