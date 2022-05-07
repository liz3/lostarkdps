const {
	app,
	BrowserWindow,
	ipcMain,
	globalShortcut,
	Tray,
	Menu,
	nativeImage,
} = require("electron");
const net = require("net");
const { execFile } = require("child_process");
const path = require("path");
const fs = require("fs");
const os = require("os");
const homedir = os.homedir();
const fPath = path.join(homedir, ".laws", ".lockfile");
const socketCreate = () => {
	return new Promise((resolve) => {
		require("dns").lookup(os.hostname(), function (err, add, fam) {
			const server = net.createServer((c) => {});
			server.listen(12345, add);
			server.close()
			resolve()
		});
	});
};
const sleep = (time) => new Promise((resolve) => setTimeout(resolve, time));
const execNative = () => {
	console.log(__dirname);
	const execPath = path.join(__dirname, "..", "native", "native_listener.exe");
	const proc = execFile(execPath, [], {
		cwd: path.join(__dirname, "..", "native"),
		shell: false,
		windowsHide: true,
	});
	proc.stdout.on("data", (data) => {
		console.log(`stdout: ${data}`);
	});
	proc.stderr.on("data", (data) => {
		console.log(`stderr: ${data}`);
	});
	proc.on("error", (data) => {
		console.log(`err: ${data}`);
	});

	return proc;
};
const getPort = () => {
	return new Promise((resolve) => {
		if (!fs.existsSync(fPath)) {
			resolve(null);
			return;
		}
		resolve(fs.readFileSync(fPath, "utf-8"));
	});
};
let visible = true;
app.whenReady().then(async () => {
	await socketCreate()
	ipcMain.handle("get_la_port", getPort);
	let proc = null;
	if (process.env.NODE_ENV !== "development") {
		proc = execNative();
		await sleep(750);
	}
	const win = new BrowserWindow({
		width: 800,
		height: 400,
		frame: false,
		autoHideMenuBar: true,
		webPreferences: {
			preload: path.join(__dirname, "preload.js"),
		},
		transparent: true,
		alwaysOnTop: true,
	});
	const ret = globalShortcut.register("CommandOrControl+Tab", () => {
		if (visible) win.hide();
		else win.show();
		visible = !visible;
	});
	const tray = new Tray(
		nativeImage.createFromPath(
			path.join(__dirname, "assets", "LostArkIcon.png")
		)
	);
	tray.on("click", () => {
		if (!visible) {
			win.show();
			visible = !visible;
		}
	});
	const trayItems = [
		{
			label: "Quit",
			click: () => {
				app.quit();
			},
		},
	];
	const menu = Menu.buildFromTemplate(trayItems);
	tray.setToolTip("LostArk Thingy");
	tray.setContextMenu(menu);
	win.setAlwaysOnTop(true, "screen");
	if (process.env.NODE_ENV === "development")
		win.loadURL("http://localhost:3000");
	else win.loadFile(path.join(__dirname, "build", "index.html"));
	app.on("will-quit", () => {
		globalShortcut.unregisterAll();
		if (proc) proc.kill();
		fs.unlinkSync(fPath);
	});
});
