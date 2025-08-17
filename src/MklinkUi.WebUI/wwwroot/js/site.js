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

async function browseFolders(textAreaId) {
    const target = document.getElementById(textAreaId);
    if (window.showDirectoryPicker) {
        try {
            const handle = await window.showDirectoryPicker();
            let path = handle.name;
            if ('path' in handle) {
                path = handle.path;
            }
            target.value += (target.value ? "\n" : "") + path;
        } catch { }
        return;
    }
    const input = document.createElement('input');
    input.type = 'file';
    input.webkitdirectory = true;
    input.multiple = true;
    input.onchange = e => {
        const dirs = new Set();
        Array.from(e.target.files).forEach(f => {
            const path = f.path || f.webkitRelativePath.split('/')[0];
            dirs.add(path);
        });
        dirs.forEach(d => {
            target.value += (target.value ? "\n" : "") + d;
        });
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
