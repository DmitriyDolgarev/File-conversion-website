import conversionConfig from "../../conversionConfig";
import Select from "react-select"


function MySelect(props) {
    
    return (
        <Select
            className="w-xs border border-fc-border-gray rounded-lg text-black bg-fc-light-gray shadow-sm hover:shadow-m"
            classNamePrefix="select"
            placeholder="Выберите вариант"
            value={props.selectedOption ? { 
            value: props.selectedOption.conversionType, 
            label: props.selectedOption.title || props.selectedOption.conversionType
            } : null}
            onChange={(selected) => {
            const selectedOption = conversionConfig[props.type]
                .find(option => option.conversionType === selected.value);
            props.setSelectedOption(selectedOption);
            }}
            options={conversionConfig[props.type]
                .filter(option => option.isArray || (props.fileCount == 1 && !option.isArray))
                .map(option => ({
                    value: option.conversionType,
                    label: option.title || option.conversionType
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
                primary: '#e5e7eb', 
                primary25: '#f3f4f6', 
                primary50: '#e5e7eb', 
            },
            })}               
        /> 
    )
}

export default MySelect