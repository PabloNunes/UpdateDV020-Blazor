window.mainPage = {
    initializeCanvas: function (canvas) {
        const ctx = canvas.getContext('2d');
        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
    },
    drawGrid: function (canvas, rows, cols, cellSize) {
        const ctx = canvas.getContext('2d');
        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < cols; col++) {
                const r = Math.floor(Math.random() * 256);
                const g = Math.floor(Math.random() * 256);
                const b = Math.floor(Math.random() * 256);
                ctx.fillStyle = `rgb(${r}, ${g}, ${b})`;
                ctx.fillRect(col * cellSize, row * cellSize, cellSize, cellSize);
            }
        }
    },
    colorCell: function (canvas, row, col, cellSize) {
        const ctx = canvas.getContext('2d');
        const r = Math.floor(Math.random() * 256);
        const g = Math.floor(Math.random() * 256);
        const b = Math.floor(Math.random() * 256);
        ctx.fillStyle = `rgb(${r}, ${g}, ${b})`;
        ctx.fillRect(col * cellSize, row * cellSize, cellSize, cellSize);
    },
    addClickEventListener: function (canvas, cellSize) {
        canvas.addEventListener('click', function (event) {
            const rect = canvas.getBoundingClientRect();
            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;
            const col = Math.floor(x / cellSize);
            const row = Math.floor(y / cellSize);
            window.mainPage.colorCell(canvas, row, col, cellSize);
        });
    },
    removeClickEventListener: function (canvas) {
        if (canvas.clickHandler) {
            canvas.removeEventListener('click', canvas.clickHandler);
            canvas.clickHandler = null;
        }
    }
};