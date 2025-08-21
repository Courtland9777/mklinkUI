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

    if (allowMultiple) {
        const input = document.createElement('input');
        input.type = 'file';
        input.webkitdirectory = true;
        input.multiple = true;
        input.onchange = e => appendFolders(target, e.target.files);
        input.click();
        return;
    }

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

            target.value = path;
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
            target.value = path;
        }
    };
    input.click();
}

function appendFolders(target, files) {
    const dirs = new Set();
    Array.from(files).forEach(f => {
        const path = (f.path || f.webkitRelativePath.split('/')[0]).replaceAll('\\', '/');
        dirs.add(path);
    });
    if (dirs.size === 0) return;
    const existing = new Set(target.value.split(/\r?\n/).filter(Boolean));
    dirs.forEach(d => {
        if (!existing.has(d)) {
            target.value += (target.value ? "\n" : "") + d;
        }
    });
}

async function dropFolders(evt) {
    evt.preventDefault();
    const target = document.getElementById('sourceFolders');
    if (!target) {
        return;
    }

    target.classList.remove('dragover');

    let files = [];

    if (evt.dataTransfer.items) {
        const items = Array.from(evt.dataTransfer.items);
        const handles = await Promise.all(items.map(async i => {
            if (i.getAsFileSystemHandle) {
                try {
                    const h = await i.getAsFileSystemHandle();
                    if (h && h.kind === 'directory') {
                        let path = h.name;
                        if ('path' in h) path = h.path;
                        return { path };
                    }
                } catch { }
            } else if (i.webkitGetAsEntry) {
                const e = i.webkitGetAsEntry();
                if (e && e.isDirectory) {
                    return { path: e.fullPath.replace(/^\//, '') };
                }
            }
            return null;
        }));
        files = handles.filter(Boolean);
    }

    if (files.length === 0 && evt.dataTransfer.files) {
        files = evt.dataTransfer.files;
    }

    appendFolders(target, files);
}

if (typeof module !== 'undefined') {
    module.exports = { appendFolders, dropFolders, browseFile, browseFolder };
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
