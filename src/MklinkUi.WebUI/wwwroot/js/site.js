// File and folder browsers
// Browsers limit access to absolute file paths for security, so only file names are available.
async function browseFile(inputId) {
    const target = document.getElementById(inputId);
    if (window.showOpenFilePicker) {
        try {
            const [handle] = await window.showOpenFilePicker({ multiple: false });
            if (handle) target.value = handle.name;
        } catch { }
        return;
    }
    const input = document.createElement('input');
    input.type = 'file';
    input.onchange = e => {
        const file = e.target.files[0];
        if (file) target.value = file.name;
    };
    input.click();
}

async function browseFolder(inputId, allowMultiple = false) {
    const target = document.getElementById(inputId);
    const input = document.createElement('input');
    input.type = 'file';

    if ('webkitdirectory' in input) {
        input.webkitdirectory = true;
        if (allowMultiple) {
            input.multiple = true;
            input.onchange = e => appendFolders(target, e.target.files);
        } else {
            input.onchange = e => {
                const file = e.target.files[0];
                if (file) {
                    // Prefer the full path when available (e.g., Electron or Chromium-based browsers)
                    const path = file.path || file.webkitRelativePath.split('/')[0];
                    target.value = path;
                }
            };
        }
        input.click();
        return;
    }

    if (!window.showDirectoryPicker) return;

    const getPath = async (handle) => {
        let path = handle.name;
        if ('path' in handle) {
            path = handle.path;
        } else if (handle.resolve && navigator.storage?.getDirectory) {
            try {
                const root = await navigator.storage.getDirectory();
                const segments = await root.resolve(handle);
                if (segments) path = segments.join('/');
            } catch { }
        }
        return path;
    };

    if (allowMultiple) {
        while (true) {
            try {
                const handle = await window.showDirectoryPicker();
                const path = await getPath(handle);
                appendFolder(target, path);
            } catch {
                break; // Cancel ends the selection loop
            }
        }
        return;
    }

    try {
        const handle = await window.showDirectoryPicker();
        target.value = await getPath(handle);
    } catch { }
}

function appendFolder(target, path) {
    const existing = new Set(target.value.split(/\r?\n/).filter(Boolean));
    const norm = path.replaceAll('\\', '/');
    if (!existing.has(norm)) {
        target.value += (target.value ? "\n" : "") + norm;
    }
}

function appendFolders(target, files) {
    const dirs = new Set();
    Array.from(files).forEach(f => {
        const path = (f.path || f.webkitRelativePath.split('/')[0]).replaceAll('\\', '/');
        dirs.add(path);
    });
    dirs.forEach(d => appendFolder(target, d));
}

if (typeof module !== 'undefined') {
    module.exports = { appendFolders };
}

function toggleInputs() {
    const isFile = document.getElementById('linkTypeFile').checked;
    document.getElementById('fileGroup').style.display = isFile ? 'block' : 'none';
    document.getElementById('folderGroup').style.display = isFile ? 'none' : 'block';
}

if (typeof window !== 'undefined') {
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
}
