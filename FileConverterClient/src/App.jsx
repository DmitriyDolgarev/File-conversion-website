import { useState } from 'react'
import './App.css'
import axios from 'axios'
import { useRef } from 'react';
import React, { useCallback } from 'react'
import { useDropzone } from 'react-dropzone'
import downloadfile from './images/downloadIcon.svg'
import MySelect from './MySelect/MySelect';
import File from './File/File'
import sync from './images/Synchronize.svg'
import Instruction from './Instruction/Instruction';
import Error from './Error/Error';



function App() {

  const [files, setFiles] = useState([]);
  const [selectedOption, setSelectedOption] = useState(null);
  const [splitParam, setSplitParam] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [isError, setIsError] = useState(false)
  const [isTypeError, setIsTypeError] = useState(false)
  const [isStringError, setIsStringError] = useState(false)
  const [downLink, setDownLink] = useState(null)
  const [resultFilename, setResultFilename] = useState("")

  const fileInputRef = useRef(null);

  const handleDelete = (filename) => {
    setFiles(files.filter(file => file.name !== filename));
  };

  const ALLOWED_EXTENSIONS = ['pdf', 'docx', 'doc', 'jpg', 'jpeg', 'png', 'pptx'];

  const onDrop = useCallback(acceptedFiles => {

    setIsError(false)
    setIsTypeError(false)

    if (acceptedFiles.length === 0) return;

    const invalidFiles = acceptedFiles.some(file => {
      const extension = file.name.split('.').pop().toLowerCase();
      return !ALLOWED_EXTENSIONS.includes(extension)
    });

    if (invalidFiles) {
      setIsError(true);
      setIsTypeError(true)
      return;
    }


    var firstExt;

    if (files.length === 0)
      firstExt = acceptedFiles[0].name.split('.').pop().toLowerCase();
    else
      firstExt = files[0].name.split('.').pop().toLowerCase();

    const allSameExt = acceptedFiles.every(file => file.name.split('.').pop().toLowerCase() === firstExt);

    if (!allSameExt) {
      setIsError(true)
      return;
    }

    setFiles(prevFiles => [...prevFiles, ...acceptedFiles]);
  }, [files])

  const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop, disabled: downLink !== null })

  const [draggedIndex, setDraggedIndex] = useState(null);



  function isValidPageInput(input) {
    const trimmed = input.trim();

    const regex = /^(\d+(-\d+)?)(\s*[,;]\s*\d+(-\d+)?)*$/;

    return regex.test(trimmed);
  }

  const toStart = () => {
    setFiles([]);
    setSplitParam("")
    setDownLink(null)
    setIsError(false)
    setIsTypeError(false)
  }

  const downloadFile = () => {
    downLink.click()
  }


  const handleClick = () => {
    if (downLink !== null) return;
    fileInputRef.current.click();
  };

  const handlerChange = (e) => {
    if (downLink !== null) return;

    setIsError(false)
    setIsTypeError(false)

    e.preventDefault();
    if (e.target.files.length === 0) return;

    const selectedFiles = Array.from(e.target.files);

    const invalidFiles = selectedFiles.some(file => {
      const extension = file.name.split('.').pop().toLowerCase();
      return !ALLOWED_EXTENSIONS.includes(extension)
    });

    if (invalidFiles) {
      setIsError(true);
      setIsTypeError(true)
      return;
    }

    const firstExt = files.length > 0
      ? files[0].name.split('.').pop().toLowerCase()
      : selectedFiles[0].name.split('.').pop().toLowerCase();

    const allSameExt = selectedFiles.every(file =>
      file.name.split('.').pop().toLowerCase() === firstExt
    );

    if (!allSameExt) {
      setIsError(true)
      return;
    }

    setFiles(prevFiles => [...prevFiles, ...selectedFiles]);

    e.target.value = null;
  }


  const sendFiles = async (e) => {
    setIsError(false)
    setIsTypeError(false);
    setIsStringError(false)

    if (selectedOption?.conversionType === 'split' && !isValidPageInput(splitParam)) {
      setIsError(true);
      setIsStringError(true);
      return;
    }

    setIsLoading(true);

    const requestBody = new FormData();

    if (selectedOption.isArray) {
      files.forEach(file => {
        requestBody.append('files', file);
      });
    }
    else {
      requestBody.append('file', files[0])
    }

    selectedOption.additionalParams.forEach(param => {
      requestBody.append(param.property, splitParam)
    });

    const response = await axios.post(`/api/${selectedOption.api}`,
      requestBody,
      {
        headers: {
          'Content-Type': 'multipart/form-data'
        },
        responseType: "blob",
      })

    setSelectedOption(null)
    setSplitParam("")

    const disposition = response.headers["content-disposition"];
    let fileName = "downloaded-file";
    if (disposition && disposition.indexOf("attachment") !== -1) {
      const matches = disposition.match("filename=(.+);");
      if (matches != null && matches[1]) {
        fileName = matches[1].replaceAll("\"", "");
      }
    }

    setResultFilename(fileName)

    const url = window.URL.createObjectURL(new Blob([response.data]));

    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", fileName);
    document.body.appendChild(link);

    setDownLink(link)
    setIsLoading(false)
  }



  return (
    <>
      <header className='bg-fc-orange text-white h-20 text-3xl lg:text-4xl font-bold py-4 pl-10 lg:pl-16'>
        File Converter
      </header>
      <div className='flex flex-col lg:flex-row mt-10 lg:mt-20 mx-10'>
        <div className='flex-3/5 h-fit'>
          <div className='rounded-xl p-5 h-92 lg:h-120 sm:mx-5 bg-fc-light-gray'>
            <div {...getRootProps()} className='rounded-xl border-1 border-dashed border-fc-dark-gray h-82 lg:h-110 p-3'>
              {files.length == 0 ?
                <div>
                  <img className={` ${isDragActive ? 'animate-bounce' : ''}  h-15 w-15 mx-auto mt-30 lg:mt-40 mb-3 cursor-pointer`} onClick={handleClick} src={downloadfile}></img>
                  <div className='text-center text-s text-fc-dark-gray'>Перетащите файл(ы) в эту область или
                    <br /> нажмите&nbsp;
                    <label className='cursor-pointer text-fc-orange underline' onClick={handleClick}>сюда
                    </label>
                    , чтобы выбрать файлы
                  </div>
                </div>
                :
                <div className="container h-106 overflow-y-auto">
                  <div className="grid grid-cols-2 lg:grid-cols-5 gap-2">
                    {files.map((file, index) => (
                      <div
                        key={file.name}
                        className="flex flex-col items-center"
                        draggable
                        onDragStart={() => setDraggedIndex(index)}
                        onDragOver={(e) => e.preventDefault()}
                        onDrop={() => {
                          const updatedFiles = [...files];
                          const draggedFile = updatedFiles[draggedIndex];

                          updatedFiles.splice(draggedIndex, 1);
                          updatedFiles.splice(index, 0, draggedFile);
                          setFiles(updatedFiles);
                          setDraggedIndex(null);
                        }}
                      >
                        <File filename={file.name} isDel={downLink === null} onDelete={handleDelete} />
                      </div>
                    ))}
                  </div>
                </div>
              }
              <input
                {...getInputProps()}
                type='file'
                className='hidden'
                multiple={true}
                onChange={handlerChange}
                ref={fileInputRef}
              />
            </div>
          </div>
          <div className='h-10'>
            <Error isError={isError} isTypeError={isTypeError} isStringError={isStringError} />
          </div>
        </div>

        <div className='flex flex-2/5 flex-col items-center text-fc-gray my-auto text-lg'>
          <div className={`${files.length > 0 ? 'order-1' : 'order-2'}  lg:order-1`}>
            {files.length > 0 ?
              <div className='space-y-2'>
                {!isLoading ?
                  <div>
                    {downLink == null ?
                      <div className='mt-10 lg:mt-14'>
                        <MySelect setIsError={setIsError} type={files[0].name.split('.').pop()} fileCount={files.length} selectedOption={selectedOption} setSelectedOption={setSelectedOption} />
                        <div className={`lg:my-15 my-10 flex items-center ${selectedOption?.conversionType === 'split' ? '' : 'invisible'}`}>
                          <span className="text-lg ml-1 mr-3 text-fc-dark-gray">Введите страницы:</span>
                          <input
                            placeholder="1-3; 5, 9-7"
                            className="bg-fc-light-gray focus:outline-none text-lg py-1 pl-2 ml-1 rounded-lg border border-gray-300 w-35"
                            onChange={(e) => setSplitParam(e.target.value.trim())}
                            type="text"
                            value={splitParam}
                          />
                        </div>
                      </div>
                      :
                      <div>
                        <div className='mx-11 mb-10 lg:mb-13 text-fc-dark-gray text-base'>Файл успешно сконвертирован!</div>
                        <div className='mx-30 mt-10 cursor-pointer' onClick={downloadFile}>
                          <File filename={resultFilename} isDel={false}></File>
                        </div>
                        <div className='mt-10 mx-12 mb-6 text-fc-dark-gray text-base'>нажмите&nbsp;
                          <label className='cursor-pointer text-fc-orange underline' onClick={downloadFile}>здесь
                          </label>
                          , чтобы скачать
                        </div>
                      </div>
                    }
                  </div>
                  :
                  <div className='mb-10 lg:mt-20 lg:mb-17'>
                    <img src={sync} className='mx-30 h-20 spin-animation' ></img>
                    <div className='mx-25 mt-6 text-fc-gray'>Конвертируем..</div>
                  </div>
                }
              </div>
              :
              <Instruction />
            }
          </div>
          <div className={`${files.length > 0 ? 'order-2' : 'order-1'}  lg:order-2`}>
            {downLink == null ?
              <button onClick={sendFiles} className={files.length == 0 || isLoading ? 'passive-button' : 'active-button'}>Конвертировать</button>
              :
              <button onClick={toStart} className='active-button'>В начало</button>
            }
          </div>
        </div>
      </div>
    </>
  )
}

export default App
