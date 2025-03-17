import { useState } from 'react'
import './App.css'

function App() {

  const [files, setFiles] = useState([]);

  const handlerChange = (e) => {
    e.preventDefault();
    if (e.target.files && e.target.files[0]){
      setFiles([...e.target.files])
    }
  }

  return (
    <>
    <header className='bg-fc-orange text-white h-20 text-4xl font-bold py-4 pl-16'>
      File Converter
    </header>
    <div className='grid grid-cols-2 gap-4 mx-10 mt-20'>
      <div className='bg-gray-300/75  rounded-xl p-5 h-100'>
        <form className='form rounded-xl border-1 border-dashed h-90 p-20'>
          <h1 className='text-center'>Бросать сюда</h1>
          <input type='file' className='input' multiple={true}
          onChange={handlerChange}
          />
        {files.length>0 && <ul className='file-list'>
          {files.map(({name}, id) =>(
            <li key={id}>{name}</li>
          ))}
          </ul>}
        </form>
      </div>
      <div>
        File Converter предоставляет возможность  конвертации
          pdf в word и jpg 
          word в pdf
          jpg в png и pdf 
          png в jpg
          PowerPoint в pdf
        А так же:
          Объединение pdf 
        Разделение pdf
      </div>
    </div>
    </>      
  )
}

export default App
