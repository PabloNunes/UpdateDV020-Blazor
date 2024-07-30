window.LangtonsAnt = {
    initializeCanvas: function (canvas) {
        let ctx = canvas.getContext('2d');
        ctx = canvas.getContext('2d');
        ctx.fillStyle = '#Ece7d7';
        ctx.fillRect(10, 10, 500, 500);
    },
    drawCanvas: function (canvas) {
        let ctx = canvas.getContext('2d');
        ctx.beginPath();
        ctx.arc(Math.floor(Math.random() * (500 + 1)), Math.floor(Math.random() * (500 + 1)), 50, 0, Math.PI * 2, true); // Circle
        ctx.fillStyle = 'red';
        ctx.fill();
    },
    saveFile: function (filename, content) {
        const blob = new Blob([content], { type: 'application/json' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },
    clearCanvas: function (canvas) {
        let ctx = canvas.getContext('2d');
        ctx.clearRect(10, 10, canvas.width, canvas.height);
    },

    drawPixel: function (canvas, posX, posY, width, height, color) {
        let ctx = canvas.getContext('2d');
        let cubeSize = 20;

        // just to debug
        //ctx.globalAlpha = 0.25

        posX = posX * cubeSize;
        posY = posY * cubeSize;
        width = width * cubeSize;
        height = height * cubeSize;

        ctx.fillStyle = color;
        ctx.fillRect(posX, posY, width, height);
    }
};