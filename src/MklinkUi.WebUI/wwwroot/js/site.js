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
            let path = handle.name;

            // Non-standard: some browsers expose a full path on the handle
            if ('path' in handle) {
                path = handle.path;
            } else if (handle.resolve && navigator.storage?.getDirectory) {
                try {
                    const root = await navigator.storage.getDirectory();
                    const segments = await root.resolve(handle);
                    if (segments) path = segments.join('/');
                } catch { }
            }

            document.getElementById(inputId).value = path;
        } catch { }
        return;
    }
    const input = document.createElement('input');
    input.type = 'file';
    input.webkitdirectory = true;
    input.onchange = e => {
        const file = e.target.files[0];
        if (file) {
            // Prefer the full path when available (e.g., Electron or Chromium-based browsers)
            const path = file.path || file.webkitRelativePath.split('/')[0];
            document.getElementById(inputId).value = path;
        }
    };
    input.click();
}

function toggleInputs() {
    const isFile = document.getElementById('linkTypeFile').checked;
    document.getElementById('fileInputs').style.display = isFile ? 'block' : 'none';
    document.getElementById('fileDest').style.display = isFile ? 'block' : 'none';
    document.getElementById('folderSource').style.display = isFile ? 'none' : 'block';
    document.getElementById('folderDest').style.display = isFile ? 'none' : 'block';
}

window.addEventListener('load', () => {
    document.getElementById('linkTypeFile').addEventListener('change', toggleInputs);
    document.getElementById('linkTypeFolder').addEventListener('change', toggleInputs);
    toggleInputs();
});
