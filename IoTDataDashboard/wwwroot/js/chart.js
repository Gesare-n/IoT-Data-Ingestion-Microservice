window.drawChart = (canvas, chartData) => {
    if (canvas && chartData) {
        const ctx = canvas.getContext('2d');
        
        // Clear previous chart
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        
        // Get chart type from data
        const chartType = chartData.type || 'line';
        
        switch (chartType) {
            case 'line':
                drawLineChart(ctx, canvas, chartData);
                break;
            case 'bar':
                drawBarChart(ctx, canvas, chartData);
                break;
            case 'gauge':
                drawGaugeChart(ctx, canvas, chartData);
                break;
            default:
                drawLineChart(ctx, canvas, chartData);
        }
    }
};

function drawLineChart(ctx, canvas, chartData) {
    const width = canvas.width;
    const height = canvas.height;
    const padding = 40;
    
    // Get data
    const labels = chartData.labels;
    const data = chartData.datasets[0].data;
    const borderColor = chartData.datasets[0].borderColor;
    const backgroundColor = chartData.datasets[0].backgroundColor;
    
    if (data.length === 0) return;
    
    // Find min and max values
    const min = Math.min(...data);
    const max = Math.max(...data);
    const range = max - min || 1; // Avoid division by zero
    
    // Draw background
    ctx.fillStyle = backgroundColor;
    ctx.fillRect(padding, padding, width - 2 * padding, height - 2 * padding);
    
    // Draw grid lines
    ctx.strokeStyle = 'rgba(0, 0, 0, 0.1)';
    ctx.lineWidth = 1;
    
    // Vertical grid lines
    for (let i = 0; i < labels.length; i++) {
        const x = padding + (i * (width - 2 * padding) / (labels.length - 1));
        ctx.beginPath();
        ctx.moveTo(x, padding);
        ctx.lineTo(x, height - padding);
        ctx.stroke();
    }
    
    // Draw line
    ctx.beginPath();
    ctx.strokeStyle = borderColor;
    ctx.lineWidth = 3;
    ctx.lineJoin = 'round';
    ctx.lineCap = 'round';
    
    for (let i = 0; i < data.length; i++) {
        const x = padding + (i * (width - 2 * padding) / (data.length - 1));
        const y = height - padding - ((data[i] - min) / range) * (height - 2 * padding);
        
        if (i === 0) {
            ctx.moveTo(x, y);
        } else {
            ctx.lineTo(x, y);
        }
    }
    ctx.stroke();
    
    // Draw points
    ctx.fillStyle = borderColor;
    for (let i = 0; i < data.length; i++) {
        const x = padding + (i * (width - 2 * padding) / (data.length - 1));
        const y = height - padding - ((data[i] - min) / range) * (height - 2 * padding);
        
        ctx.beginPath();
        ctx.arc(x, y, 5, 0, Math.PI * 2);
        ctx.fill();
    }
    
    // Draw labels
    ctx.fillStyle = 'black';
    ctx.font = '12px Arial';
    ctx.textAlign = 'center';
    
    // X-axis labels
    for (let i = 0; i < labels.length; i++) {
        const x = padding + (i * (width - 2 * padding) / (labels.length - 1));
        ctx.fillText(labels[i], x, height - 10);
    }
    
    // Y-axis labels
    ctx.textAlign = 'right';
    ctx.textBaseline = 'middle';
    for (let i = 0; i <= 5; i++) {
        const value = min + (i / 5) * range;
        const y = height - padding - (i / 5) * (height - 2 * padding);
        ctx.fillText(value.toFixed(1), padding - 10, y);
    }
}

function drawBarChart(ctx, canvas, chartData) {
    const width = canvas.width;
    const height = canvas.height;
    const padding = 40;
    
    // Get data
    const labels = chartData.labels;
    const data = chartData.datasets[0].data;
    const backgroundColor = chartData.datasets[0].backgroundColor;
    const borderColor = chartData.datasets[0].borderColor;
    
    if (data.length === 0) return;
    
    // Find min and max values
    const min = Math.min(...data);
    const max = Math.max(...data);
    const range = max - min || 1; // Avoid division by zero
    
    // Calculate bar width
    const barWidth = (width - 2 * padding) / data.length * 0.8;
    const barSpacing = (width - 2 * padding) / data.length * 0.2;
    
    // Draw bars
    for (let i = 0; i < data.length; i++) {
        const x = padding + i * (barWidth + barSpacing);
        const barHeight = ((data[i] - min) / range) * (height - 2 * padding);
        const y = height - padding - barHeight;
        
        // Draw bar
        ctx.fillStyle = backgroundColor;
        ctx.fillRect(x, y, barWidth, barHeight);
        
        // Draw border
        ctx.strokeStyle = borderColor;
        ctx.lineWidth = 1;
        ctx.strokeRect(x, y, barWidth, barHeight);
    }
    
    // Draw labels
    ctx.fillStyle = 'black';
    ctx.font = '12px Arial';
    ctx.textAlign = 'center';
    
    // X-axis labels
    for (let i = 0; i < labels.length; i++) {
        const x = padding + i * (barWidth + barSpacing) + barWidth / 2;
        ctx.fillText(labels[i], x, height - 10);
    }
    
    // Y-axis labels
    ctx.textAlign = 'right';
    ctx.textBaseline = 'middle';
    for (let i = 0; i <= 5; i++) {
        const value = min + (i / 5) * range;
        const y = height - padding - (i / 5) * (height - 2 * padding);
        ctx.fillText(value.toFixed(1), padding - 10, y);
    }
}

function drawGaugeChart(ctx, canvas, chartData) {
    const width = canvas.width;
    const height = canvas.height;
    
    // Get data
    const value = chartData.value;
    const min = chartData.min || 0;
    const max = chartData.max || 100;
    const label = chartData.label || '';
    const unit = chartData.unit || '';
    
    // Calculate gauge dimensions
    const centerX = width / 2;
    const centerY = height / 2;
    const radius = Math.min(width, height) / 2 - 20;
    
    // Draw gauge background
    ctx.beginPath();
    ctx.arc(centerX, centerY, radius, Math.PI, 0, false);
    ctx.strokeStyle = '#e0e0e0';
    ctx.lineWidth = 20;
    ctx.stroke();
    
    // Draw gauge fill
    const valueRange = max - min;
    const valuePercent = (value - min) / valueRange;
    const endAngle = Math.PI + (valuePercent * Math.PI);
    
    ctx.beginPath();
    ctx.arc(centerX, centerY, radius, Math.PI, endAngle, false);
    ctx.strokeStyle = valuePercent > 0.7 ? '#ff6b35' : '#00d9ff';
    ctx.lineWidth = 20;
    ctx.stroke();
    
    // Draw value text
    ctx.fillStyle = 'black';
    ctx.font = 'bold 24px Arial';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(`${value.toFixed(1)} ${unit}`, centerX, centerY - 10);
    
    // Draw label
    ctx.font = '16px Arial';
    ctx.fillText(label, centerX, centerY + 20);
    
    // Draw min/max labels
    ctx.font = '12px Arial';
    ctx.textAlign = 'left';
    ctx.fillText(min.toString(), centerX - radius, centerY + 40);
    ctx.textAlign = 'right';
    ctx.fillText(max.toString(), centerX + radius, centerY + 40);
}