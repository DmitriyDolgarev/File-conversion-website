import { useState } from 'react'
import './App.css'
import axios from 'axios'
import conversionConfig from '../conversionConfig';
import Select from "react-select"

function App() {

  const [files, setFiles] = useState([]);
  const [selectedOption, setSelectedOption] = useState(null);



  const handlerChange = (e) => {
    e.preventDefault();
    if (e.target.files && e.target.files[0]){
      setFiles([...e.target.files])
    }
  }


  const sendFiles = async (e) =>{

    const requestBody =  {}

    if (selectedOption.isArray){
      requestBody.files = files
    }
    else{
      requestBody.file = files[0]
    }

    selectedOption.additionalParams.forEach(param => {
      requestBody[param.property] = 2
    });

    const response = await axios.post(`/api/${selectedOption.api}`,
      requestBody,
      {
        headers:{
          'Content-Type': 'multipart/form-data'
        },
        responseType: "blob",
      }) 

      const disposition = response.headers["content-disposition"];
      let fileName = "downloaded-file";
      if (disposition && disposition.indexOf("attachment") !== -1) {
        const matches = disposition.match("filename=(.+);");
        console.log(matches)
        if (matches != null && matches[1]) {
          fileName = matches[1].replaceAll("\"","");
        }
      }
      

      const url = window.URL.createObjectURL(new Blob([response.data]));

      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", fileName);
      document.body.appendChild(link);
      link.click();
  }

  return (
    <>
    <header className='bg-fc-orange text-white h-20 text-4xl font-bold py-4 pl-16'>
      File Converter
    </header>
      <div className='flex flex-row my-20'>
      <div className=' flex-3/5 bg-fc-light-gray rounded-xl p-5 h-120 mx-20'>
        <form className='form rounded-xl border-1 border-dashed border-fc-dark-gray h-110 p-20'>
          <h1 className='text-center text-fc-dark-gray'>Бросать сюда</h1>
          <input type='file' className='input text-fc-dark-gray' multiple={true}
          onChange={handlerChange}
          />
        {files.length>0 && <ul className='file-list text-fc-dark-gray'>
          {files.map(({name}, id) =>(
            <li key={id}>{name}</li>
          ))}
          </ul>}
        </form>
      </div>
      <div className='flex-2/5 text-fc-gray my-auto ml-20 text-lg'>
        <div className={files.length==0 ? '': 'hidden'}>
          <ul className='list-disc'>
                File Converter предоставляет<br/>
                возможность  конвертации:
            <li className='ml-7'>pdf в word и jpg</li>
            <li className='ml-7'>word в pdf</li>
            <li className='ml-7'>jpg в png и pdf</li> 
            <li className='ml-7'>png в jpg</li> 
            <li className='ml-7'>PowerPoint в pdf</li> 
          </ul>
          <ul className='list-disc'>А так же:
            <li className='ml-7'>Объединение pdf</li>
            <li className='ml-7'>Разделение pdf</li>
          </ul> 
        </div>
        {files.length>0 ? 
          <div className='space-y-2 p-4'>
              <Select
                className="w-xs border border-fc-border-gray rounded-lg text-black bg-fc-light-gray shadow-sm hover:shadow-m"
                classNamePrefix="select"
                placeholder="Выберите вариант"
                value={selectedOption ? { 
                  value: selectedOption.conversionType, 
                  label: selectedOption.conversionType 
                } : null}
                onChange={(selected) => {
                  const selectedOption = conversionConfig[files[0].name.split('.').pop()]
                    .find(option => option.conversionType === selected.value);
                  setSelectedOption(selectedOption);
                }}
                options={conversionConfig[files[0].name.split('.').pop()].map(option => ({
                  value: option.conversionType,
                  label: option.conversionType
                }))}
                styles={{
                  dropdownIndicator: (provided, state) => ({
                    ...provided,
                    color: '#868686',
                    svg: {
                      width: "30px", 
                      height: "30px",
                    },
                    transition: "transform 0.1s ease",
                    transform: state.selectProps.menuIsOpen ? "rotate(180deg)" : "rotate(0deg)",
                    '&:hover': {
                      color: '#868686',
                    },
                  }),
                  control: (provided) => ({
                    ...provided,
                    padding: '0.5rem',
                    minHeight: 'auto',
                    '&:hover': {
                      boxShadow: '0 0 0 2px rgba(0, 0, 0, 0.1)',
                    },
                  }),
                  menu: (provided) => ({
                    ...provided,
                    borderRadius: '0.5rem',
                    marginTop: '0.25rem',
                  }),
                  option: (provided, state) => ({
                    ...provided,
                    backgroundColor: state.isFocused ? '#f3f4f6' : 'white',
                    color: 'black',
                    '&:active': {
                      backgroundColor: '#e5e7eb',
                    },
                  }),
                }}
                theme={(theme) => ({
                  ...theme,
                  colors: {
                    ...theme.colors,
                    primary: '#e5e7eb', // focus border color
                    primary25: '#f3f4f6', // option hover color
                    primary50: '#e5e7eb', // option active color
                  },
                })}
              />
          </div> 
        : ""
        }
        
      <button onClick={sendFiles} className='cursor-pointer bg-fc-orange rounded-lg hover:bg-fc-orange/80 text-white w-55 h-13 mx-5 my-10 font-bold text-lg'>Конвертировать</button>
      </div>
    </div>
    </>      
  )
}

export default App
