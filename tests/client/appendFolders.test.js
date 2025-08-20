const assert = require('assert');
const { appendFolders } = require('../../src/MklinkUi.WebUI/wwwroot/js/site.js');

const target = { value: 'C:/existing' };
appendFolders(target, []);
assert.strictEqual(target.value, 'C:/existing');

appendFolders(target, [
  { path: 'C:/new', webkitRelativePath: 'new/file.txt' },
  { path: 'C:/existing', webkitRelativePath: 'existing/other.txt' }
]);
assert.strictEqual(target.value, 'C:/existing\nC:/new');

console.log('appendFolders tests passed');
