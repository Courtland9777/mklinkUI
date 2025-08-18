const assert = require('assert');
const { appendFolders } = require('../../src/MklinkUi.WebUI/wwwroot/js/site.js');

const target = { value: 'C:/existing' };
appendFolders(target, []);
assert.strictEqual(target.value, 'C:/existing');

console.log('appendFolders cancel test passed');
