import { create } from "zustand";

export const usePromptStore = create((set) => ({
    firstText: "",
    secondText: "",
    setFirstText: (text) => set({ firstText: text }),
    setSecondText: (text) => set({ secondText: text }),
    getCombinedText: () =>
        usePromptStore.getState().firstText + " " + usePromptStore.getState().secondText
}));