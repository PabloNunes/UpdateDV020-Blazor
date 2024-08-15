window.LangtonsAnt = {

    BlazorReference: null,
    ReferenceCache: function (componentReference) {
        LangtonsAnt.BlazorReference = componentReference;
    },
    initializeCanvas: function (canvas) { },
    saveFileDialog: async function (filename, json) {
        try {
            const opts = {
                suggestedName: filename,
                types: [{
                    description: 'JSON Files',
                    accept: { 'application/json': ['.json'] }
                }]
            };

            const handle = await window.showSaveFilePicker(opts);
            const writable = await handle.createWritable();
            const blob = new Blob([json], { type: 'application/json' });

            // Will have a wait for the data to be written and to be closed.
            await writable.write(blob);
            await writable.close()
        } catch (err) {
            console.error('Error during file save dialog:', err);
            return null;
        }
    },
    
    clearCanvas: function (canvas) {
        let ctx = canvas.getContext('2d');
        ctx.clearRect(10, 10, canvas.width, canvas.height);
    },

    handleCanvasClick: function (canvas) {
        canvas.clickHandler = function (event) {
            let rect = canvas.getBoundingClientRect();
            let x = event.clientX - rect.left;
            let y = event.clientY - rect.top;

            let cubeSize = 20;
            let posX = Math.floor(x / cubeSize);
            let posY = Math.floor(y / cubeSize);

            LangtonsAnt.BlazorReference.invokeMethodAsync('HandleCanvasClick', posX, posY);
        };
        canvas.addEventListener('click', canvas.clickHandler);
    },

    removeClickEventListener: function (canvas) {
        canvas.removeEventListener('click', canvas.clickHandler);
        canvas.clickHandler = null;
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