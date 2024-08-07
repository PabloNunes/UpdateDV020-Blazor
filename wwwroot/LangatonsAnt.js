window.LangtonsAnt = {
    initializeCanvas: function (canvas) {},
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

    handleCanvasClick: function (canvas) {
        canvas.addEventListener('click', function (event) {
            let rect = canvas.getBoundingClientRect();
            let x = event.clientX - rect.left;
            let y = event.clientY - rect.top;

            let cubeSize = 20;
            let posX = Math.floor(x / cubeSize);
            let posY = Math.floor(y / cubeSize);

            DotNet.invokeMethodAsync('LangtonsAntBlazorFluent', 'HandleCanvasClick', posX, posY);
        });
    },

    removeClickEventListener: function (canvas) {
        if (canvas.clickHandler) {
            canvas.removeEventListener('click', canvas.clickHandler);
            canvas.clickHandler = null;
        }
    },

    drawPixel: function (canvas, posX, posY, width, height, color) {
        let ctx = canvas.getContext('2d');
        let cubeSize = 20;

        posX = posX * cubeSize;
        posY = posY * cubeSize;
        width = width * cubeSize;
        height = height * cubeSize;

        ctx.fillStyle = color;
        ctx.fillRect(posX, posY, width, height);
    }
};