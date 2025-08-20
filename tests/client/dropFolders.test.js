const assert = require('assert');
const { dropFolders } = require('../../src/MklinkUi.WebUI/wwwroot/js/site.js');

let prevented = false;
const target = { value: '', classList: { remove: () => {} } };

global.document = { getElementById: () => target };

dropFolders({ preventDefault: () => { prevented = true; }, dataTransfer: { files: [{ path: 'C:/new' }] } });

assert.strictEqual(target.value, 'C:/new');
assert.strictEqual(prevented, true);

console.log('dropFolders test passed');
