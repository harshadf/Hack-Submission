import React, { useEffect, useState } from 'react';
import { Box, Typography, Paper, List, ListItem, ListItemText, TextField, Button } from '@mui/material';
import { usePromptStore } from './store/usePromptStore';
import axios from '../src/axios';

function ChatBox() {
  const [chatMessages, setChatMessages] = useState([]);
  const [chatInput, setChatInput] = useState('');

  const secondText = usePromptStore((state) => state.secondText);
  const setSecondText = usePromptStore((state) => state.setSecondText);
  const getCombinedText = usePromptStore((state) => state.getCombinedText);
  const [aiResponse, setAiResponse] = useState("");

  useEffect(() => {
    const prompt = getCombinedText();
    const userMessage = { from: 'user', text: prompt };
    setChatMessages((prev) => [...prev, userMessage]);
    setSecondText('');

    // Simulate AI response after a short delay
    setTimeout(() => {
      const aiResponseText = { from: 'ai', text: `${aiResponse}` };
      setChatMessages((prev) => [...prev, aiResponseText]);
    }, 500);
  }, [aiResponse]);

  const handleSendChat = async () => {
    const prompt = getCombinedText();
    console.log("prompt text: ", prompt);

    // if (secondText.trim() === '') return;

    await axios.post(`/ChatWithAI/`, { prompt }, {
      headers: {
        'Content-Type': 'application/json'
      }
    })
    .then((res) => {
      console.log('res: ', res);
      setAiResponse(res.data);
      console.log('aiResponse state: ', aiResponse);
    })
    .catch((er) => {
      console.log(er);
    })
  };

  return (
    <Box sx={{ 
      height: '100%', 
      width: '100vh', 
      display: 'flex', 
      flexDirection: 'column', 
      alignItems: 'center', 
      justifyContent: 'flex-start', 
      p: 2 
    }}>
      
      <Box sx={{ 
        width: '95%',
        maxWidth: '1400px',
        height: '100%', 
        display: 'flex', 
        flexDirection: 'column' 
      }}>
        
        <Typography variant="h5" gutterBottom>
          Chat with AI Agent
        </Typography>

        <Paper 
          variant="outlined" 
          sx={{ 
            flex: 1, 
            overflowY: 'auto', 
            p: 2, 
            mb: 2, 
            display: 'flex', 
            flexDirection: 'column',
            minHeight: '400px'
          }}
        >
          <List>
            {chatMessages.map((msg, idx) => (
              <ListItem 
                key={idx} 
                sx={{
                  display: 'flex',
                  justifyContent: msg.from === 'user' ? 'flex-end' : 'flex-start',
                }}
              >
                <Box 
                  sx={{
                    bgcolor: msg.from === 'user' ? '#1976d2' : '#e0e0e0',
                    color: msg.from === 'user' ? '#fff' : '#000',
                    px: 2,
                    py: 1,
                    borderRadius: 2,
                    maxWidth: '70%',
                    wordBreak: 'break-word',
                  }}
                >
                  {msg.text}
                </Box>
              </ListItem>
            ))}
          </List>
        </Paper>

        <Box display="flex" gap={1}>
          <TextField
            fullWidth
            variant="outlined"
            size="small"
            value={secondText}
            onChange={(e) => setSecondText(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSendChat()}
          />
          <Button
            variant="contained"
            onClick={handleSendChat}
            disabled={secondText.trim() === ''}
          >
            Send
          </Button>
        </Box>

      </Box>
    </Box>
  );
}

export default ChatBox;
