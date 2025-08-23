const assert = require('assert');
const { appendFolders } = require('../../src/MklinkUi.WebUI/wwwroot/js/site.js');

const target = { value: 'C:/existing' };
appendFolders(target, []);
assert.strictEqual(target.value, 'C:/existing');

const targetWithPath = { value: '' };
const files = [{
    path: 'C:/base/folder/sub/file.txt',
    webkitRelativePath: 'folder/sub/file.txt'
}];
appendFolders(targetWithPath, files);
assert.strictEqual(targetWithPath.value, 'C:/base/folder');

console.log('appendFolders tests passed');
