// File and folder browsers
// Browsers limit access to absolute file paths for security, so only file names are available.
async function browseFile(inputId, allowMultiple = false) {
    const target = document.getElementById(inputId);
    if (window.showOpenFilePicker) {
        try {
            const handles = await window.showOpenFilePicker({ multiple: allowMultiple });
            handles.forEach(h => {
                target.value += (target.value ? "\n" : "") + h.name;
            });
        } catch { }
        return;
    }
    const input = document.createElement('input');
    input.type = 'file';
    if (allowMultiple) input.multiple = true;
    input.onchange = e => {
        Array.from(e.target.files).forEach(file => {
            target.value += (target.value ? "\n" : "") + file.name;
        });
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
