using Microsoft.AspNetCore.Components;

namespace IoTDataDashboard.Components
{
    /// <summary>
    /// A reusable card component for displaying statistics
    /// </summary>
    public partial class StatisticsCard : ComponentBase
    {
        /// <summary>
        /// The title of the card
        /// </summary>
        [Parameter]
        public string? Title { get; set; }

        /// <summary>
        /// The main value to display
        /// </summary>
        [Parameter]
        public string? Value { get; set; }

        /// <summary>
        /// Optional subtitle or additional information
        /// </summary>
        [Parameter]
        public string? Subtitle { get; set; }
    }
}