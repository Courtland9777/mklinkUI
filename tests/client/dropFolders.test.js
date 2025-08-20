const assert = require('assert');
const { dropFolders } = require('../../src/MklinkUi.WebUI/wwwroot/js/site.js');

let prevented = false;
const target = { value: '', classList: { remove: () => {} } };

global.document = { getElementById: () => target };

// Existing FileList support
dropFolders({ preventDefault: () => { prevented = true; }, dataTransfer: { files: [{ path: 'C:/new' }] } });
assert.strictEqual(target.value, 'C:/new');
assert.strictEqual(prevented, true);

// Reset for DataTransferItemList scenario
prevented = false;
target.value = '';

const items = [{
    webkitGetAsEntry: () => ({ isDirectory: true, fullPath: '/C/dir' })
}];

dropFolders({ preventDefault: () => { prevented = true; }, dataTransfer: { files: [], items } });

assert.strictEqual(target.value, 'C/dir');
assert.strictEqual(prevented, true);

console.log('dropFolders test passed');
