export const fileToByteArray = (file: File): Promise<Uint8Array> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        if (reader.result instanceof ArrayBuffer) {
          resolve(new Uint8Array(reader.result));
        } else {
          reject('Неверный формат файла');
        }
      };
      reader.onerror = reject;
      reader.readAsArrayBuffer(file);
    });
  };
  