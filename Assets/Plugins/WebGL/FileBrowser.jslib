mergeInto(LibraryManager.library, {

  DownloadFile: function (filenamePtr, dataPtr) {
    const filename = UTF8ToString(filenamePtr);
    const data = UTF8ToString(dataPtr);

    const blob = new Blob([data], { type: "application/json" });
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    URL.revokeObjectURL(url);
  },

  OpenFile: function (gameObjectNamePtr) {
    const gameObjectName = UTF8ToString(gameObjectNamePtr);

    const input = document.createElement("input");
    input.type = "file";
    input.accept = ".artrack";
    input.multiple = false;

    input.onchange = () => {
      const file = input.files[0];
      if (!file) return;

      const name = file.name.toLowerCase();
      if (!name.endsWith(".artrack")) {
          alert("Please select a valid .artrack file.");
          return;
      }

      const reader = new FileReader();
      reader.onload = () => {
        SendMessage(gameObjectName, "OnFileLoaded", reader.result);
      };
      reader.readAsText(file);
    };

    input.click();
  }

});
