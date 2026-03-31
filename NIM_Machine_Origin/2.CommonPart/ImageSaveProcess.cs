using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MachineControlBase
{
    /// <summary>
    /// 비동기 이미지 저장 처리 클래스
    /// </summary>
    public class ImageSaveProcess
    {
        /// <summary>
        /// ConcurrentQueue에 기반한 BlockingCollection으로 만든 큐
        /// </summary>
        private readonly BlockingCollection<ImageData> ferQueue = new BlockingCollection<ImageData>(new ConcurrentQueue<ImageData>());

        /// <summary>
        /// Queue에 이미지가 들어오면 저장하는 스레드
        /// </summary>
        private Thread threadImageFileSave = null;

        /// <summary>
        /// 스레드 반복 실행 플래그
        /// </summary>
        private bool bThreadEnable = true;

        /// <summary>
        /// 생성자
        /// </summary>
        public ImageSaveProcess()
        {
            // Thread 생성
            threadImageFileSave = new Thread(new ThreadStart(Worker));
            threadImageFileSave.Name = "ImageFileSaveThread";
            threadImageFileSave.Priority = ThreadPriority.Lowest;
            threadImageFileSave.IsBackground = true;
            threadImageFileSave.Start();
        }

        /// <summary>
        /// 이미지 저장 스레드 함수
        /// </summary>
        private void Worker()
        {
            ImageData imageData = null;
            while (bThreadEnable)
            {
                try
                {
                    ferQueue.TryTake(out imageData, -1);  // -1을 줘서 Queue에 데이터가 들어올때까지 무한 대기
                    Parallel.Invoke(() =>
                    {
                        if (imageData != null)
                        {
                            if (imageData.iImageType == eImageType.BMP &&
                                imageData.ImageFile != null)
                            {
                                CogImageFile ImageFile = new CogImageFile();
                                CXMLProcess.CreateFolder(imageData.strPath);
                                ImageFile.Open(imageData.strFileName, CogImageFileModeConstants.Write);
                                ImageFile.Append(imageData.ImageFile);
                                ImageFile.Close();
                            }
                            else if (imageData.iImageType == eImageType.IDB &&
                                     imageData.ImageFile != null)
                            {
                                CogImageFileCDB cogImageFileCDB = new CogImageFileCDB();
                                CXMLProcess.CreateFolder(imageData.strPath);
                                cogImageFileCDB.Open(imageData.strFileName, CogImageFileModeConstants.Write);
                                cogImageFileCDB.Append(imageData.ImageFile);
                                cogImageFileCDB.Close();
                            }
                            else if (imageData.iImageType == eImageType.BARCORD &&
                                     imageData.bitmap != null)
                            {
                                CXMLProcess.CreateFolder(imageData.strPath);
                                imageData.bitmap.Save(imageData.strFileName, ImageFormat.Jpeg);
                                imageData.bitmap.Dispose();
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, "ImageSaveProcess : " + ex.ToString(), false);
                }
            }
        }

        /// <summary>
        /// 여러 스레드에서 이미지 받는 함수
        /// </summary>
        /// <param name="imageData"></param>
        public void SetSaveImage(ImageData imageData)
        {
            ferQueue.TryAdd(imageData, -1);
        }

        /// <summary>
        /// 스레드와 큐를 해제합니다.
        /// </summary>
        /// <returns></returns>
        public bool free()
        {
            if (ferQueue.Count != 0) return false;
            bThreadEnable = false;
            ferQueue.Dispose();
            return true;
        }
    }

    /// <summary>
    /// 이미지 저장 데이터 클래스
    /// </summary>
    public class ImageData
    {
        /// <summary>
        /// 저장 경로명
        /// </summary>
        public string strPath = string.Empty;

        /// <summary>
        /// 파일명이 포함된 저장 경로명
        /// </summary>
        public string strFileName = string.Empty;

        /// <summary>
        /// 이미지 저장 타입
        /// 0 : BMP
        /// 1 : IDB
        /// 2 : BARCORD
        /// </summary>
        public eImageType iImageType = 0;

        /// <summary>
        /// 저장 이미지
        /// </summary>
        public ICogImage ImageFile = null;

        /// <summary>
        /// 바코드 이미지 저장
        /// </summary>
        public Bitmap bitmap = null;
    }

    /// <summary>
    /// 이미지 타입 정의
    /// </summary>
    public enum eImageType
    {
        BMP = 0,
        IDB,
        BARCORD,
    }
}