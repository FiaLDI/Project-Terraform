const fs = require("fs");
const path = require("path");
const ignore = require("ignore");

// --- CONFIG ---
const OUTPUT_FILE = path.join(__dirname, "assets-tree.json");

// --- PATH DETECTION ---
const projectRoot = path.resolve(__dirname, "../../");
const assetsRoot = path.join(projectRoot, "Assets");
const gitignorePath = path.join(projectRoot, ".gitignore");

// --- LOAD .gitignore ---
if (!fs.existsSync(gitignorePath)) {
    console.error("❌ .gitignore НЕ найден по пути:", gitignorePath);
    process.exit(1);
}

const gitignoreContent = fs.readFileSync(gitignorePath, "utf8");
const ig = ignore().add(gitignoreContent);

// --- SCAN FUNCTION ---
async function scanDir(dir) {
    const results = [];

    const entries = await fs.promises.readdir(dir, { withFileTypes: true });

    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        const relativePath = fullPath.replace(projectRoot + path.sep, "");

        // Skip ignored by .gitignore
        if (ig.ignores(relativePath)) continue;

        if (entry.isDirectory()) {
            results.push({
                type: "folder",
                name: entry.name,
                path: relativePath,
                children: await scanDir(fullPath)
            });
        } else {
            results.push({
                type: "file",
                name: entry.name,
                path: relativePath
            });
        }
    }

    return results;
}

// --- MAIN ---
(async () => {
    console.log("📁 Сканирую:", assetsRoot);

    const tree = await scanDir(assetsRoot);

    // write JSON file
    fs.writeFileSync(OUTPUT_FILE, JSON.stringify(tree, null, 2), "utf8");

    console.log("✅ Готово!");
    console.log("📄 JSON сохранён в:", OUTPUT_FILE);
})();
