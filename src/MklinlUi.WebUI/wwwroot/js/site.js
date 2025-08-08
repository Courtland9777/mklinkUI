// File and folder browsers
async function browseFile(inputId) {
    if (window.showOpenFilePicker) {
        try {
            const [handle] = await window.showOpenFilePicker();
            document.getElementById(inputId).value = handle.name;
        } catch { }
        return;
    }
    const input = document.createElement('input');
    input.type = 'file';
    input.onchange = e => {
        const file = e.target.files[0];
        if (file) document.getElementById(inputId).value = file.name;
    };
    input.click();
}

async function browseFolder(inputId) {
    if (window.showDirectoryPicker) {
        try {
            const handle = await window.showDirectoryPicker();
            document.getElementById(inputId).value = handle.name;
        } catch { }
        return;
    }
    const input = document.createElement('input');
    input.type = 'file';
    input.webkitdirectory = true;
    input.onchange = e => {
        const file = e.target.files[0];
        if (file) document.getElementById(inputId).value = file.webkitRelativePath.split('/')[0];
    };
    input.click();
}
