const assert = require('assert');
const { dropFolders } = require('../../src/MklinkUi.WebUI/wwwroot/js/site.js');

let prevented = false;
const target = { value: '', classList: { remove: () => {} } };

global.document = { getElementById: () => target };

(async () => {
    // Existing FileList support
    await dropFolders({ preventDefault: () => { prevented = true; }, dataTransfer: { files: [{ path: 'C:/new' }] } });
    assert.strictEqual(target.value, 'C:/new');
    assert.strictEqual(prevented, true);

    // Reset for DataTransferItemList scenario
    prevented = false;
    target.value = '';

    const items = [{
        webkitGetAsEntry: () => ({ isDirectory: true, fullPath: '/C/dir' })
    }];

    await dropFolders({ preventDefault: () => { prevented = true; }, dataTransfer: { files: [], items } });

    assert.strictEqual(target.value, 'C/dir');
    assert.strictEqual(prevented, true);

    // Support getAsFileSystemHandle
    prevented = false;
    target.value = '';

    const handleItems = [{
        getAsFileSystemHandle: async () => ({ kind: 'directory', name: 'C', path: 'C:/handleDir' })
    }];

    await dropFolders({ preventDefault: () => { prevented = true; }, dataTransfer: { files: [], items: handleItems } });

    assert.strictEqual(target.value, 'C:/handleDir');
    assert.strictEqual(prevented, true);

    // Prefer items when File objects lack path data
    prevented = false;
    target.value = '';

    const fileItems = [{ name: 'file.txt', webkitRelativePath: '' }];
    const itemWithHandle = [{
        getAsFileSystemHandle: async () => ({ kind: 'directory', name: 'D', path: 'D:/fromItems' })
    }];

    await dropFolders({
        preventDefault: () => { prevented = true; },
        dataTransfer: { files: fileItems, items: itemWithHandle }
    });

    assert.strictEqual(target.value, 'D:/fromItems');
    assert.strictEqual(prevented, true);

    console.log('dropFolders test passed');
})().catch(err => {
    console.error(err);
    process.exit(1);
});
