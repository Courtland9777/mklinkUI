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

    const submitButton = document.getElementById('submitButton');
    const spinner = document.getElementById('submitSpinner');
    if (submitButton && spinner) {
        const form = submitButton.closest('form');
        if (form) {
            form.addEventListener('submit', () => {
                submitButton.disabled = true;
                spinner.classList.remove('d-none');
            });
        }
        submitButton.disabled = false;
        spinner.classList.add('d-none');
    }
});
