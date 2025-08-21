const assert = require('assert');
const { browseFile, browseFolder } = require('../../src/MklinkUi.WebUI/wwwroot/js/site.js');

const target = { value: 'initial', classList: { remove: () => {} } };

global.document = { getElementById: () => target };
global.window = {};
global.navigator = {};

(async () => {
    // File picker cancellation should not change the input or throw
    window.showOpenFilePicker = async () => { throw new Error('AbortError'); };
    await browseFile('input');
    assert.strictEqual(target.value, 'initial');

    // Folder picker cancellation should not change the input or throw
    window.showDirectoryPicker = async () => { throw new Error('AbortError'); };
    await browseFolder('input');
    assert.strictEqual(target.value, 'initial');

    console.log('browse pickers cancel test passed');
})().catch(err => {
    console.error(err);
    process.exit(1);
});
