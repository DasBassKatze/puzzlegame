using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Выбор и загрузка изображения пользователя (PNG, JPG).
/// </summary>
public static class ImageLoader
{
    /// <summary>Открывает диалог выбора файла. Возвращает путь или null, если файл не выбран.</summary>
    public static string PickImage()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(
            "Выберите изображение", "", new[] { "Изображения", "png,jpg,jpeg" });
        return string.IsNullOrEmpty(path) ? null : path;
#elif UNITY_STANDALONE_WIN
        return WindowsFileDialog.OpenImage();
#else
        return null;
#endif
    }

    /// <summary>Читает файл и создаёт из него текстуру. Возвращает null, если файл не является изображением.</summary>
    public static Texture2D LoadTexture(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;

        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Не удалось прочитать файл {path}: {e.Message}");
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.name = Path.GetFileNameWithoutExtension(path);
        return texture;
    }
}

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
/// <summary>
/// Стандартный диалог открытия файла Windows через функцию GetOpenFileName из comdlg32.dll.
/// </summary>
internal static class WindowsFileDialog
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private class OpenFileName
    {
        public int structSize;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public string filter;
        public string customFilter;
        public int maxCustFilter;
        public int filterIndex;
        public string file;
        public int maxFile;
        public string fileTitle;
        public int maxFileTitle;
        public string initialDir;
        public string title;
        public int flags;
        public short fileOffset;
        public short fileExtension;
        public string defExt;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public string templateName;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt;
        public int flagsEx;
    }

    private const int OFN_EXPLORER = 0x00080000;
    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_NOCHANGEDIR = 0x00000008;

    [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    public static string OpenImage()
    {
        var ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.dlgOwner = GetActiveWindow();
        ofn.filter = "Изображения (*.png;*.jpg;*.jpeg)\0*.png;*.jpg;*.jpeg\0\0";
        ofn.file = new string(new char[1024]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[256]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        ofn.title = "Выберите изображение";
        ofn.flags = OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR;

        if (!GetOpenFileName(ofn))
            return null;
        return ofn.file.TrimEnd('\0');
    }
}
#endif
