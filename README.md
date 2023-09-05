# SPRNetTool


Test method

 private byte[]? EncryptFrameData(PaletteColour[] pixelArray, PaletteColour[] paletteData
            , int frameWidth, int frameHeigth, int frameOffX, int frameOffY)
        {
            
            var encryptedFrameDataList = new List<byte>();

            var frameInfo = new FrameInfo();
            frameInfo.OffX = (short)frameOffX;
            frameInfo.OffY = (short)frameOffY;
            frameInfo.Height = (short)frameHeigth;
            frameInfo.Width = (short)frameWidth;
            frameInfo.CopyStructToListArray(encryptedFrameDataList);

            for (int i = 0; i < pixelArray.Length;)
            {
                byte size = 0;
                byte alpha = pixelArray[i].Alpha;
                if (alpha == 0)
                {
                    while (pixelArray[i].Alpha == 0 && size < frameWidth && size < 255)
                    {
                        i++;
                        size++;
                    }
                    encryptedFrameDataList.Add(size);
                    encryptedFrameDataList.Add(alpha);
                }
                else
                {
                    List<byte> temp = new List<byte>();
                    while (pixelArray[i].Alpha != 0 && size < frameWidth && size < 255)
                    {
                        byte index = 0x00;// TODO: pixelArray[i] ở trong paletteData
                        temp.Add(index);
                        i++;
                        size++;
                    }
                    encryptedFrameDataList.Add(size);
                    encryptedFrameDataList.AddRange(temp);
                }
            }
            return encryptedFrameDataList.ToArray();
        }
