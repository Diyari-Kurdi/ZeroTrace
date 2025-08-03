# 📛 ZeroTrace

**ZeroTrace** is a cross-platform, Ahead-of-Time (AOT) compiled secure file deletion tool for Windows, Linux, and macOS. It safely overwrites and deletes files, folders, and partitions using multiple custom patterns to help prevent data recovery.

<p align="center">
  <img width="1024" height="1024" alt="ZeroTrace Banner" src="https://github.com/user-attachments/assets/fc92e997-9e12-4640-bbf2-35a2cd918554" />
</p>

## 📄 About

ZeroTrace is designed to securely erase sensitive data from your storage devices. It uses multiple overwriting techniques, including zero fill, random data, byte shaking, and more, to make file recovery extremely difficult. The tool provides a user-friendly console interface with progress feedback and supports both file/folder and partition-level deletion.  
ZeroTrace is built with .NET 9 and supports AOT compilation for fast startup and reduced runtime dependencies.

## 📚 Table of Contents

- [About](#-about)
- [Features](#-features)
- [Warning](#️-warning)
- [Getting Started](#-getting-started)
- [Usage](#-usage)
- [License](#-license)

## ✨ Features

- **Multiple Overwrite Passes:** Overwrites data using multiple passes with different secure patterns.
- **Custom Patterns:** Includes zero fill, random data, reverse, and byte shaking methods.
- **Partition Wiping:** Fills free space on partitions to remove traces of deleted files.
- **Cross-Platform:** Runs on Windows, Linux, and macOS*.
- **Ahead-of-Time (AOT) Compilation:** Fast startup and minimal runtime dependencies.
- **Open Source:** Licensed under MIT.

---

## ⚠️ Warning

> ### **SSD Limitations**
>
> While **ZeroTrace** securely overwrites data on traditional hard drives (HDDs), it **cannot guarantee complete erasure on SSDs or flash-based storage**. Due to **wear leveling**, **over-provisioning**, and **internal caching**, data may persist in areas not accessible to software.
> For **highly sensitive data** on SSDs, consider using **manufacturer-provided secure erase tools** or **physical destruction**.

> ⚠️ Use with caution. **Data deleted with ZeroTrace cannot be recovered.**
> Always **double-check selected paths** before confirming.

---

## 🚀 Getting Started

### 📦 Download Prebuilt Releases

Visit the [GitHub Releases page](https://github.com/Diyari-Kurdi/ZeroTrace/releases) and download the latest release for your platform.

- 🪟 **Windows**: Download the `.exe` file and double-click to run.
- 🐧 **Linux**: Download the binary, then run:

```bash
chmod +x ZeroTrace-linux-x64
./ZeroTrace-linux-x64
````

### 🔧 Build from Source

```sh
git clone https://github.com/Diyari-Kurdi/ZeroTrace.git
cd ZeroTrace
```

Ensure you have [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed.

```sh
dotnet build -c Release
```

### Run the Application

```sh
dotnet run --project ZeroTrace
```

## 📖 Usage

1. **Select Target Type:** Choose to securely delete files/folders or wipe a partition.
2. **Add Targets:** Select or enter paths for files, folders, or partitions.
3. **Confirm Deletion:** Review selected targets and confirm the operation.
4. **Monitor Progress:** Monitor the progress as ZeroTrace securely erases your data.



### 📸 Screenshot

<img width="1501" height="913" alt="image" src="https://github.com/user-attachments/assets/a1169d0a-0ded-40de-8bc2-951f63555ebf" />

---

### 📹 Demo Video

Watch ZeroTrace in action:

https://github.com/user-attachments/assets/b1d4bb9d-4a7f-4aea-8f26-b788d3281cef

---

## 📃 License

ZeroTrace is licensed under the [MIT License](https://github.com/Diyari-Kurdi/ZeroTrace?tab=MIT-1-ov-file).

---


## 💖 Support This Project

If you find **ZeroTrace** useful and want to support continued development, consider donating:

| Coin | Address                                                                                      | QR Code                                  |
|----------------|----------------------------------------------------------------------------------------------|------------------------------------------|
| Bitcoin | `1A1pm2DNMFtsBHzKea64HCogLKMpScagZu`                                                       | ![BTC QR](https://github.com/user-attachments/assets/a4bb3518-3aeb-4d66-84c4-f387ecbaba40)           |
| Monero | `897p7tTp8BRGUY92XYvUcCABmWfMpSQqa2APFK28wwHsC6b3RaUVPQ1EYyJ66jqLhxWdBLe9FvcHoAEn7K2PYhrAS7SJKGw` | ![XMR QR](https://github.com/user-attachments/assets/ab7ffec8-ac46-4ffd-8cdc-8cd72621974f)           |

Thank you for your support! ❤️

