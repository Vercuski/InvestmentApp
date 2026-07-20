// Thin wrapper around Chart.js for the ticker price chart on the Chart page.
// Dataset order is fixed and relied on by Chart.razor's checkbox indices:
//   0 = Open, 1 = High, 2 = Low, 3 = Close, 4 = Buy signals, 5 = Sell signals
window.stockChart = (function () {
    let chart = null;

    function buildDatasets(payload) {
        const visible = Array.isArray(payload.visible) ? payload.visible : [];
        const isHidden = (index) => visible.length > index ? !visible[index] : false;

        const isFullConfidence = (context) => {
            const point = context.raw;
            return !!point
                && typeof point.confidencePercent === 'number'
                && Math.round(point.confidencePercent) >= 100;
        };

        const highlightBorderColor = (defaultColor) => (context) =>
            isFullConfidence(context) ? 'rgb(255, 255, 0)' : defaultColor;

        const highlightBorderWidth = (defaultWidth) => (context) =>
            isFullConfidence(context) ? 3 : defaultWidth;

        return [
            {
                label: 'Open',
                data: payload.open,
                type: 'line',
                borderColor: 'rgb(0, 153, 51)',
                backgroundColor: 'rgb(0, 153, 51)',
                pointRadius: 0,
                borderWidth: 2,
                order: 4,
                hidden: isHidden(0)
            },
            {
                label: 'High',
                data: payload.high,
                type: 'line',
                borderColor: 'rgb(0, 102, 204)',
                backgroundColor: 'rgb(0, 102, 204)',
                pointRadius: 0,
                borderWidth: 2,
                order: 3,
                hidden: isHidden(1)
            },
            {
                label: 'Low',
                data: payload.low,
                type: 'line',
                borderColor: 'rgb(0, 0, 0)',
                backgroundColor: 'rgb(0, 0, 0)',
                pointRadius: 0,
                borderWidth: 2,
                order: 2,
                hidden: isHidden(2)
            },
            {
                label: 'Close',
                data: payload.close,
                type: 'line',
                borderColor: 'rgb(204, 0, 0)',
                backgroundColor: 'rgb(204, 0, 0)',
                pointRadius: 0,
                borderWidth: 2,
                order: 1,
                hidden: isHidden(3)
            },
            {
                label: 'Buy signal',
                data: payload.buys,
                type: 'scatter',
                showLine: false,
                pointStyle: 'triangle',
                rotation: 0,
                pointRadius: 7,
                pointHoverRadius: 9,
                backgroundColor: 'rgb(0, 153, 51)',
                borderColor: highlightBorderColor('rgb(0, 90, 30)'),
                borderWidth: highlightBorderWidth(1),
                order: 0,
                hidden: isHidden(4)
            },
            {
                label: 'Sell signal',
                data: payload.sells,
                type: 'scatter',
                showLine: false,
                pointStyle: 'triangle',
                rotation: 180,
                pointRadius: 7,
                pointHoverRadius: 9,
                backgroundColor: 'rgb(204, 0, 0)',
                borderColor: highlightBorderColor('rgb(120, 0, 0)'),
                borderWidth: highlightBorderWidth(1),
                order: 0,
                hidden: isHidden(5)
            }
        ];
    }

    function render(canvasId, payload) {
        destroy();

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            return;
        }

        chart = new Chart(canvas, {
            type: 'line',
            data: {
                labels: payload.labels,
                datasets: buildDatasets(payload)
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false,
                interaction: {
                    mode: 'nearest',
                    axis: 'x',
                    intersect: false
                },
                scales: {
                    x: {
                        type: 'category',
                        ticks: {
                            maxRotation: 0,
                            autoSkip: true
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Price'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const label = context.dataset.label || '';
                                const value = context.parsed.y;
                                const priceText = value == null ? '' : value.toFixed(2);
                                const confidencePercent = context.raw && typeof context.raw.confidencePercent === 'number'
                                    ? context.raw.confidencePercent
                                    : null;

                                if (confidencePercent != null) {
                                    return `${label}: ${priceText} (confidence ${confidencePercent.toFixed(0)}%)`;
                                }

                                return value == null ? label : `${label}: ${priceText}`;
                            }
                        }
                    }
                }
            }
        });
    }

    function setDatasetVisibility(canvasId, datasetIndex, visible) {
        if (!chart) {
            return;
        }
        chart.setDatasetVisibility(datasetIndex, visible);
        chart.update();
    }

    function destroy() {
        if (chart) {
            chart.destroy();
            chart = null;
        }
    }

    return { render, setDatasetVisibility, destroy };
})();