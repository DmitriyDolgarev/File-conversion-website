const conversionConfig = {
    "png":[
        {
            "conversionType": "jpg",
            "api": "images/pngToJpg",
            "isArray": true,
            "additionalParams": []
        }        
    ],           
    "jpg":[
            {
                "conversionType": "png",
                "api": "images/jpgToPng",
                "isArray": true,
                "additionalParams": []
            },
            
            {
                "conversionType": "pdf",
                "api": "pdf/jpgToPdf",
                "isArray": true,
                "additionalParams": []
            }
        
    ],
    "pdf":[
        {
                "conversionType": "jpg",
                "api": "pdf/pdfToJpg",
                "isArray": true,
                "additionalParams": []
        },

        {
            "conversionType": "word",
            "api": "office/pdfToWord",
            "isArray": true,
            "additionalParams": []
                
        },
        {
            "conversionType": "merge",
            "api": "pdf/merge",
            "isArray": true,
            "additionalParams": []
        },
        {
            "conversionType": "split",
                "api": "pdf/split",
                "isArray": false,
                "additionalParams": [
                    {
                        "title": "Страница разделения",
                        "property": "SplitString",
                        "type": "string"
                    }
                ]        
        }
    ],
    "doc":[
        {
            "conversionType": "pdf",
            "api": "office/wordToPdf",
            "isArray": true,
            "additionalParams": []
        }      
    ],
    "ppt":[
        {
            "conversionType": "pdf",
            "api": "office/pptxToPdf",
            "isArray": true,
            "additionalParams": []
        }    
    ]   
}

conversionConfig.docx = conversionConfig.doc
conversionConfig.pptx = conversionConfig.ppt

export default conversionConfig