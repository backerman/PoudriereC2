import {
    ActionButton,
    CommandBar,
    ContextualMenu,
    DefaultButton,
    Dialog,
    DialogFooter,
    DialogType,
    ICommandBarItemProps,
    IDialogContentProps,
    IModalProps,
    ITextField,
    PrimaryButton,
    TextField
} from "@fluentui/react";
import { ItemList } from '../ItemList';

/** Properties for a {@link ConfigCommandBar}.
*/
export interface ConfigCommandBarProps {
    /** Text to use as the label for the add dialog box's confirm button */
    addConfirmButtonText: string;
    /** Whether the add dialog box should currently be hidden */
    addDialogHidden: boolean;
    /** A reference to the text field in the add dialog box, for obtaining the
     * newly created item's name
     */
    addNameRef: React.RefObject<ITextField>;
    /** Whether the delete button should currently be disabled */
    deleteButtonDisabled: boolean;
    /** Whether the delete dialog box should currently be hidden */
    deleteDialogHidden: boolean;
    /** Function to call when the add dialog box should be hidden */
    hideAddDialog: () => void;
    /** Function to call when the delete dialog box should be hidden */
    hideDeleteDialog: () => void;
    /** Function to call when the add dialog box's confirm button is clicked */
    onAddConfirmClick: () => void;
    /** Function to call when the delete dialog box's confirm button is clicked */
    onDeleteConfirmClick: () => void;
    /** The type of item displayed in the {@link ItemList} (plural form) */
    pluralItemName: string;
    /** Function to call when the add dialog box should be shown */
    showAddDialog: () => void;
    /** Function to call when the delete dialog box should be shown */
    showDeleteDialog: () => void;
    /** The type of item displayed in the {@link ItemList} (singular form) */
    singularItemName: string;
}

const draggableProps: IModalProps = {
    dragOptions: {
        moveMenuItemText: 'Move',
        closeMenuItemText: 'Close',
        menu: ContextualMenu
    }
}

/** Command bar for an {@link ItemList}; contains add and delete buttons. */
export function ConfigCommandBar(props: ConfigCommandBarProps): JSX.Element {
    const {
        addConfirmButtonText,
        addDialogHidden,
        addNameRef,
        deleteButtonDisabled,
        deleteDialogHidden,
        hideAddDialog,
        hideDeleteDialog,
        onAddConfirmClick,
        onDeleteConfirmClick,
        pluralItemName,
        showAddDialog,
        showDeleteDialog,
        singularItemName
    } = props;

    const commandBarItems: ICommandBarItemProps[] = [
        {
            key: 'add',
            text: 'Add',
            iconProps: { iconName: 'Add' },
            onClick: showAddDialog
        },
        {
            key: 'delete',
            text: 'Delete',
            iconProps: { iconName: 'Delete' },
            ariaDescription: `Delete selected ${pluralItemName}`,
            disabled: deleteButtonDisabled,
            onClick: showDeleteDialog
        }
    ]

    const addContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: `Create ${singularItemName}`,
        subText: `Please specify the name of the ${singularItemName} to be created.`
    }

    const deleteConfirmProps: IDialogContentProps = {
        type: DialogType.normal,
        title: `Delete ${pluralItemName}`,
        subText: `Are you sure you want to delete the selected ${pluralItemName}?`
    }

    return (
        <>
            <CommandBar
                items={commandBarItems} />
            <Dialog
                hidden={addDialogHidden}
                modalProps={draggableProps}
                dialogContentProps={addContentProps}
                onDismiss={hideAddDialog}>
                <TextField label={"Name"} componentRef={addNameRef} />
                <DialogFooter>
                    <DefaultButton onClick={onAddConfirmClick} text={addConfirmButtonText} />
                    <ActionButton onClick={hideAddDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
            <Dialog
                hidden={deleteDialogHidden}
                modalProps={draggableProps}
                dialogContentProps={deleteConfirmProps}
                onDismiss={hideDeleteDialog} >
                <DialogFooter>
                    <PrimaryButton onClick={onDeleteConfirmClick} text="Delete" />
                    <DefaultButton onClick={hideDeleteDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
        </>
    )
}
